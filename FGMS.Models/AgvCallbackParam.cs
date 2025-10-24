namespace FGMS.Models
{
    public class AgvCallbackParam
    {
        /// <summary>
        /// 请求编号，每个请求都要一个唯一编号， 同一个请求重复提交， 使用同一编号
        /// </summary>
        public string ReqCode { get; set; }
        /// <summary>
        /// 请求时间戳，格式: “yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string ReqTime { get; set; }
        /// <summary>
        /// 地码 X 坐标(mm)：任务完成时有值
        /// </summary>
        public string? CooX { get; set; }
        /// <summary>
        /// 地码 Y 坐标(mm)：任务完成时有值
        /// </summary>
        public string? CooY { get; set; }
        /// <summary>
        /// 当前位置编号
        /// 任务开始：该位置为任务起点
        /// 走出储位：该位置为任务起点
        /// 任务单取消：该位置为工作位编号
        /// 任务结束：该位置为任务终点
        /// </summary>
        public string CurrentPositionCode { get; set; }
        /// <summary>
        /// 方法名, 可使用任务类型做为方法名
        /// 由RCS-2000任务模板配置后并告知上层系统
        /// 默认使用方式:
        /// start : 任务开始
        /// outbin : 走出储位
        /// end : 任务结束[给MOM，值传0, 1, 2；3成功；4失败]
        /// cancel : 任务单取消
        /// apply: ctu取放料箱申请
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// AGV编号（同 agvCode ）
        /// </summary>
        public string RobotCode { get; set; }
        /// <summary>
        /// 当前任务单号
        /// </summary>
        public string TaskCode { get; set; }
    }
}
