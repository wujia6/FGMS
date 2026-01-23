function signalrInit()
{
    const hubUrl = "http://10.39.0.184:12307/hubs/agvHubService";

    connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    // 注册事件处理
    connection.on('agvTaskExecuted', function (data) {
        console.log('AGV任务已执行');
        // 刷新任务列表
    });

    connection.on('agvCallbackReceived', function (data) {
        console.log('收到AGV回调通知');
        // 刷新任务列表
    });

    // 开始连接
    connection.start().then(() => {
        console.log('SignalR连接成功');
    }).catch(err => {
        console.error('SignalR连接失败:', err);
    });
}

signalrInit();
