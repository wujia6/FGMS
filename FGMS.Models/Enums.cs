namespace FGMS.Models
{
    public enum ElementCategory
    {
        砂轮轴 = 1,
        砂轮 = 2
    }

    public enum ElementUnit
    {
        个 = 1,
        片 = 2,
    }

    public enum WorkOrderType
    {
        砂轮申领 = 1,
        砂轮返修 = 2,
        砂轮退仓 = 3,
        机台更换 = 4
    }

    public enum WorkOrderStatus
    {
        待审 = 0,
        驳回 = 1,
        审核通过 = 2,
        砂轮整备 = 3,
        参数修整 = 4,
        整备完成 = 5,
        工单配送 = 6,
        返修配送 = 7,
        机台接收 = 8,
        退仓配送 = 9,
        工单结束 = 10,
        AGV收料 = 11,
        呼叫AGV = 12,
        取消 = 13,
        挂起 = 14
    }

    public enum WorkOrderPriority
    {
        低 = 1,
        中 = 2,
        高 = 3
    }

    public enum ElementEntityStatus
    {
        待入库 = 0,
        在库 = 1,
        出库 = 2,
        上机 = 3,
        下机 = 4,
        返修 = 5,
        报废 = 6
    }

    public enum DiscardReason
    {
        调机报废 = 1,
        正常损耗 = 2,
        来料异常 = 3,
        标准变更 = 4,
        存储异常 = 5
    }

    public enum LogType
    {
        出入库 = 1,
        整修 = 2,
        返修 = 3,
        上下机 = 4,
        其他 = 5
    }
}
