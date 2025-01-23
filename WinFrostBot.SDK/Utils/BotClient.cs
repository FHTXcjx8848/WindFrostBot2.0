using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using WindFrostBot.SDK;

public class BotClient
{
    private static string ?appId;
    private static string ?clientSecret;
    private static string ?accessToken;
    private static string gatewayUrl = "wss://api.sgroup.qq.com/websocket";
    private static int heartbeatInterval;
    private static string ?sessionId;
    private static int sequenceNumber;
    private static ClientWebSocket ?_webSocket;

    public BotClient(string appid, string clientsecret)
    {
        clientSecret = clientsecret;
        appId = appid;
        new Task(async () => { await Init(); }).Start();
    }
    private static async Task UpdateTokenAsync()
    {
        await GetAccessToken();
    }
    public async Task Init()
    {
        await GetAccessToken();
        await ConnectWebSocket();
    }

    public static async Task GetAccessToken()
    {
        using (var client = new HttpClient())
        {
            var requestContent = new StringContent(
                $"{{\"appId\": \"{appId}\", \"clientSecret\": \"{clientSecret}\"}}",
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("https://bots.qq.com/app/getAppAccessToken", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            var tokenResponse = JObject.Parse(responseString);
            accessToken = tokenResponse["access_token"].ToString();

            //Message.Info($"Access Token: {accessToken}");
        }
    }

    public async Task ConnectWebSocket()
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(gatewayUrl), CancellationToken.None);
        Message.Info("Connection opened.");
        SendIdentify();//发送鉴定
        new Task(async () =>
        {
            var buffer = new byte[1024 * 4];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //Message.Info("Received Message: " + message);
                try
                {
                    var data = JObject.Parse(message);
                    ProcessMessage(data);
                }
                catch
                {
                    Message.Info("Received Message: " + message);
                    _webSocket.Dispose();
                    await ConnectWebSocket();
                }
            }
        }, TaskCreationOptions.LongRunning).Start();
    }

    public static async void SendIdentify()
    {
        var identifyPayload = new JObject
        {
            ["op"] = 2,
            ["d"] = new JObject
            {
                ["token"] = "QQBot " + accessToken,
                ["intents"] = 1 << 25,
                ["shard"] = new JArray { 0, 1 },
                ["properties"] = new JObject
                {
                    ["$os"] = "windows",
                    ["$browser"] = "my_library",
                    ["$device"] = "my_library"
                }
            }
        };

        await SendMessageAsync(identifyPayload.ToString());
        Message.Info("Identify sent.");
    }

    private async void ProcessMessage(JObject message)
    {
        int op = message["op"].Value<int>();
        switch (op)
        {
            case 10://心跳包
                heartbeatInterval = message["d"]["heartbeat_interval"].Value<int>();
                var heartbeatPayload = new JObject
                {
                    ["op"] = 1,
                    ["d"] = sequenceNumber
                };
                await SendMessageAsync(heartbeatPayload.ToString());
                break;
            case 0://群聊消息
                sequenceNumber = message["s"].Value<int>();
                string eventType = message["t"].Value<string>();
                switch (eventType)
                {
                    case "READY"://准备完成后的事件
                        sessionId = message["d"]["session_id"].Value<string>();
                        break;
                    case "GROUP_AT_MESSAGE_CREATE"://群消息事件
                        HandleGroupAtMessageCreate(message["d"]);
                        break;
                    case "GROUP_ADD_ROBOT"://机器人被添加到群的事件
                        break;
                    case "GROUP_DEL_ROBOT"://机器人被移除群聊事件
                        break;
                    case "GROUP_MSG_REJECT"://群聊主动消息关闭事件
                        break;
                    case "GROUP_MSG_RECEIVE"://群聊主动消息开启事件
                        break;
                    case "FRIEND_ADD"://私聊好友添加事件
                        break;
                    case "FRIEND_DEL"://私聊好友删除事件
                        break;
                }
                break;
            case 9://处理无效的会话
                Message.Info("Received INVALID_SESSION");
                // await ConnectWebSocket();
                break;
            case 11:
                //心跳包成功后返回
                break;
        }
    }
    /*
    public static async Task StartHeartbeat()
    {
        while (_webSocket.State == WebSocketState.Open)
        {
            var heartbeatPayload = new JObject
            {
                ["op"] = 1,
                ["d"] = sequenceNumber
            };

            await SendMessageAsync(heartbeatPayload.ToString());
            //Message.Info("Heartbeat sent.");
            await Task.Delay(heartbeatInterval);
        }
    }
    */
    public static async Task SendMessageAsync(string message)
    {
        var messageBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public void HandleGroupAtMessageCreate(JToken data)//处理群聊被动消息部分
    {
        var author = data["author"]["member_openid"].Value<string>();
        var content = data["content"].Value<string>();
        var groupOpenId = data["group_openid"].Value<string>();
        var msgId = data["id"].Value<string>(); // 获取消息ID

        Message.Info($"Received @ message from {author} in group {groupOpenId}: {content}");

        RaiseOnMessageReceived(new MessageEventArgs(groupOpenId, content, msgId, author));
        //SendMessage(groupOpenId, $"{content}", msgId);
    }
    public event EventHandler<MessageEventArgs> OnMessageReceived;
    public virtual void RaiseOnMessageReceived(MessageEventArgs e)
    {
        OnMessageReceived?.Invoke(this, e);
    }
    public async void SendMessage(string message,MessageEventArgs args,int seq = 1)
    {
        await GetAccessToken();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("QQBot", accessToken);
            client.DefaultRequestHeaders.Add("X-Union-Appid", appId);

            var postData = new JObject
            {
                ["content"] = message,
                ["msg_type"] = 0, // 文本消息
                ["msg_id"] = args.MsgId,
                ["msg_seq"] = seq
            };
            var content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://api.sgroup.qq.com/v2/groups/{args.GroupOpenId}/messages", content);
            var responseString = await response.Content.ReadAsStringAsync();

            Message.Info("Sent Message Response: " + responseString);
        }
    }
    public async Task SendMedia(MessageEventArgs args, string fileUrl,int seq = 1)
    {
        try
        {
            // Log the attempt to send the file URL
            Message.Info("尝试发送: " + fileUrl);

            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new Exception("Failed to upload the file to the server.");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("QQBot", accessToken);
                client.DefaultRequestHeaders.Add("X-Union-Appid", appId);

                // Get file info for the uploaded file
                var postData = new JObject
                {
                    ["file_type"] = 1,
                    ["url"] = fileUrl,
                    ["srv_send_msg"] = false,
                    ["msg_seq"] = seq
                };

                var content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://api.sgroup.qq.com/v2/groups/{args.GroupOpenId}/files", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                //Message.Info(responseString);

                if (string.IsNullOrEmpty(jsonResponse["file_info"]?.Value<string>()))
                {
                    throw new Exception("Image upload failed: " + jsonResponse["message"]?.Value<string>());
                }

                string fileInfo = jsonResponse["file_info"]?.Value<string>();
                if (string.IsNullOrEmpty(fileInfo))
                {
                    throw new Exception("Failed to retrieve file_info from the response.");
                }

                // Send media message to the group
                var messageData = new JObject
                {
                    //["content"] = "", // Ensure content is not null or empty
                    ["msg_id"] = args.MsgId,
                    ["msg_type"] = 7,
                    ["media"] = new JObject
                    {
                        ["file_info"] = fileInfo
                    }
                };

                var messageContent = new StringContent(messageData.ToString(), Encoding.UTF8, "application/json");
                var messageResponse = await client.PostAsync($"https://api.sgroup.qq.com/v2/groups/{args.GroupOpenId}/messages", messageContent);
                var messageResponseString = await messageResponse.Content.ReadAsStringAsync();

                Message.Info("Sent Media Message Response: " + messageResponseString);
            }
        }
        catch (Exception ex)
        {
            Message.Erro("Error: " + ex.Message);
        }
    }
    public async Task SendMedia(MessageEventArgs args, Image img, int seq = 1)
    {
        try
        {
            string fileUrl = UploadImageToServer(img).Result;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("QQBot", accessToken);
                client.DefaultRequestHeaders.Add("X-Union-Appid", appId);

                // Get file info for the uploaded file
                var postData = new JObject
                {
                    ["file_type"] = 1,
                    ["url"] = fileUrl,
                    ["srv_send_msg"] = false,
                    ["msg_seq"] = seq
                };

                var content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://api.sgroup.qq.com/v2/groups/{args.GroupOpenId}/files", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                //Message.Info(responseString);

                if (string.IsNullOrEmpty(jsonResponse["file_info"]?.Value<string>()))
                {
                    throw new Exception("Image upload failed: " + jsonResponse["message"]?.Value<string>());
                }

                string fileInfo = jsonResponse["file_info"]?.Value<string>();
                if (string.IsNullOrEmpty(fileInfo))
                {
                    throw new Exception("Failed to retrieve file_info from the response.");
                }

                // Send media message to the group
                var messageData = new JObject
                {
                    //["content"] = "", // Ensure content is not null or empty
                    ["msg_id"] = args.MsgId,
                    ["msg_type"] = 7,
                    ["media"] = new JObject
                    {
                        ["file_info"] = fileInfo
                    }
                };

                var messageContent = new StringContent(messageData.ToString(), Encoding.UTF8, "application/json");
                var messageResponse = await client.PostAsync($"https://api.sgroup.qq.com/v2/groups/{args.GroupOpenId}/messages", messageContent);
                var messageResponseString = await messageResponse.Content.ReadAsStringAsync();

                Message.Info("Sent Media Message Response: " + messageResponseString);
            }
        }
        catch (Exception ex)
        {
            Message.Erro("Error: " + ex.Message);
        }
    }
    public async Task<string> UploadImageToServer(Image image)
    {
        using (var client = new HttpClient())
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                var content = new MultipartFormDataContent();
                content.Add(fileContent, "file", "wiki.jpg");
                var response = await client.PostAsync(MainSDK.BotConfig.FileServerUrl, content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);
                return jsonResponse["fileUrl"]?.ToString();
            }
        }
    }
    public async Task<string> UploadFileToServer(string filePath)
    {
        using (var client = new HttpClient())
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await client.PostAsync(MainSDK.BotConfig.FileServerUrl, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(responseString);
            return jsonResponse["fileUrl"]?.ToString();
        }
    }

}
public class MessageEventArgs : EventArgs
{
    public string Author { get; }
    public string GroupOpenId { get; }
    public string Content { get; }
    public string MsgId { get; }

    public MessageEventArgs(string groupOpenId, string content, string msgId, string author)
    {
        GroupOpenId = groupOpenId;
        Content = content;
        MsgId = msgId;
        Author = author;
    }
}
