using FGMS.Models.Dtos;
using FGMS.Models.Entities;

namespace FGMS.Utils
{
    public class MyEvent
    {
        /// <summary>
        /// 自定义事件参数
        /// </summary>
        public class MyEventArgs : EventArgs
        {
            private readonly WorkOrderDto dto;

            public MyEventArgs(WorkOrderDto dto)
            {
                this.dto = dto;
            }

            public WorkOrderDto GetWorkOrderDto
            {
                get { return this.dto; }
            }
        }

        /// <summary>
        /// 申明事件委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void StatusChangeEventHandle(object sender, MyEventArgs args);

        /// <summary>
        /// 申明事件
        /// </summary>
        public event StatusChangeEventHandle OnStatusChangeEvent = default!;

        /// <summary>
        /// 定义触发事件
        /// </summary>
        /// <param name="dto"></param>
        public void StatusChange(WorkOrderDto dto)
        {
            var args = new MyEventArgs(dto);
            OnStatusChangeEvent?.Invoke(this, args);
        }
    }
}
