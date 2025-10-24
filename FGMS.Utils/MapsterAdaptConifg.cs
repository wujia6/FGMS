using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using Mapster;

namespace FGMS.Utils
{
    public class MapsterAdaptConifg
    {
        public static TypeAdapterConfig Initial()
        {
            var config = new TypeAdapterConfig();

            config.ForType<Brand, BrandDto>().IgnoreNullValues(true);

            config.ForType<Equipment, EquipmentDto>()
                .Map(dest => dest.OrganizeName, src => src.Organize!.Name)
                .Map(dest => dest.WorkOrderDtos, src => src.WorkOrders)
                .IgnoreNullValues(true);

            config.ForType<Organize, OrganizeDto>()
                .Map(dest => dest.ParentName, src => src.Parent!.Name)
                .Map(dest => dest.ChildrenDtos, src => src.Childrens)
                .IgnoreNullValues(true);

            config.ForType<RoleInfo, RoleInfoDto>()
                .Map(dest => dest.OrganizeName, src => src.Organize!.Name)
                .IgnoreNullValues(true);

            config.ForType<UserInfo, UserInfoDto>()
                .Map(dest => dest.RoleInfoName, src => src.RoleInfo!.Name)
                .IgnoreNullValues(true);

            config.ForType<Standard, StandardDto>()
                .Map(dest => dest.MainElementMaterialNo, src => src.MainElement == null ? string.Empty : src.MainElement!.MaterialNo)
                .Map(dest => dest.MainElementSpec, src => src.MainElement == null ? string.Empty : src.MainElement.Spec)
                .Map(dest => dest.FirstElementMaterialNo, src => src.FirstElement == null? string.Empty : src.FirstElement!.MaterialNo)
                .Map(dest => dest.FirstElementSpec, src => src.FirstElement == null? string.Empty : src.FirstElement.Spec)
                .Map(dest => dest.SecondElementMaterialNo, src => src.SecondElement == null ? string.Empty : src.SecondElement!.MaterialNo)
                .Map(dest => dest.SecondElementSpec, src => src.SecondElement == null ? string.Empty : src.SecondElement.Spec)
                .Map(dest => dest.ThirdElementMaterialNo, src => src.ThirdElement == null ? string.Empty : src.ThirdElement!.MaterialNo)
                .Map(dest => dest.ThirdElementSpec, src => src.ThirdElement == null ? string.Empty : src.ThirdElement.Spec)
                .Map(dest => dest.FourthElementMaterialNo, src => src.FourthElement == null ? string.Empty : src.FourthElement!.MaterialNo)
                .Map(dest => dest.FourthElelemtSpec, src => src.FourthElement == null ? string.Empty : src.FourthElement.Spec)
                .Map(dest => dest.FifthElementMaterialNo, src => src.FifthElement == null ? string.Empty : src.FifthElement!.MaterialNo)
                .Map(dest => dest.FifthElementSpec, src => src.FifthElement == null ? string.Empty : src.FifthElement.Spec)
                .IgnoreNullValues(true);

            config.ForType<Element, ElementDto>()
                .Map(dest => dest.BrandName, src => src.Brand!.Name)
                .Map(dest => dest.Category, src => Enum.GetName(typeof(ElementCategory), src.Category))
                .Map(dest => dest.Unit, src => Enum.GetName(typeof(ElementUnit), src.Unit))
                .Map(dest => dest.ElementEntityDtos, src => src.ElementEntities)
                .IgnoreNullValues(true);

            config.ForType<ElementEntity, ElementEntityDto>()
                .Map(dest => dest.Status, src => Enum.GetName(typeof(ElementEntityStatus), src.Status))
                .Map(dest => dest.ElementCategory, src => src.Element != null ? Enum.GetName(typeof(ElementCategory), src.Element!.Category) : string.Empty)
                .Map(dest => dest.ElementMaterialNo, src => src.Element != null? src.Element.MaterialNo : string.Empty)
                .Map(dest => dest.ElementName, src => src.Element != null ? src.Element.Name : string.Empty)
                .Map(dest => dest.ElementSpec, src => src.Element != null ? src.Element.Spec : string.Empty)
                .Map(dest => dest.ComponentCode, src => src.Component != null ? src.Component!.Code : string.Empty)
                .Map(dest => dest.WorkOrderNo, src => src.Component!.WorkOrder!.OrderNo)
                .Map(dest => dest.CargoSpaceCode, src => src.CargoSpace != null ? src.CargoSpace!.Code : string.Empty)
                .Map(dest => dest.CargoSpaceName, src => src.CargoSpace != null ? src.CargoSpace!.Name : string.Empty)
                .Map(dest => dest.CargoSpaceQuantity, src => src.CargoSpace != null ? src.CargoSpace!.Quantity : new int?())
                .IgnoreNullValues(true);

            config.ForType<Component, ComponentDto>()
                .Map(dest => dest.StandardCode, src => src.Standard != null ? src.Standard!.Code : string.Empty)
                .Map(dest => dest.WorkOrderNo, src => src.WorkOrder != null ? src.WorkOrder!.OrderNo : string.Empty)
                .Map(dest => dest.Status, src => Enum.GetName(typeof(ElementEntityStatus), src.Status))
                .Map(dest => dest.CargoSpaceCode, src => src.CargoSpace != null ? src.CargoSpace!.Code : string.Empty)
                .Map(dest => dest.CargoSpaceName, src => src.CargoSpace != null ? src.CargoSpace!.Name : string.Empty)
                .Map(dest => dest.CargoSpaceQuantity, src => src.CargoSpace != null ? src.CargoSpace!.Quantity : new int?())
                .Map(dest => dest.StandardDto, src => src.Standard ?? null)
                .Map(dest => dest.ElementEntityDtos, src => src.ElementEntities)
                .IgnoreNullValues(true);

            config.ForType<ComponentDto, Component>()
                .Map(dest => dest.ElementEntities, src => src.ElementEntityDtos)
                .IgnoreNullValues(true);

            config.ForType<WorkOrder, WorkOrderDto>()
                .Map(dest => dest.ParentNo, src => src.Parent!.OrderNo)
                .Map(dest => dest.EquipmentCode, src => src.Equipment!.Code)
                .Map(dest => dest.OrganizeCode, src => src.Equipment!.Organize!.Code)
                .Map(dest => dest.UserInfoName, src => src.UserInfo!.Name)
                .Map(dest => dest.Type, src => Enum.GetName(typeof(WorkOrderType), src.Type))
                .Map(dest => dest.Priority, src => Enum.GetName(typeof(WorkOrderPriority), src.Priority))
                .Map(dest => dest.Status, src => Enum.GetName(typeof(WorkOrderStatus), src.Status))
                .Map(dest => dest.StandardDtos, src => src.WorkOrderStandards != null? src.WorkOrderStandards!.Select(s => s.Standard) : null)
                .Map(dest => dest.ComponentDtos, src => src.Components)
                .Map(dest => dest.ChildrenDtos, src => src.Childrens)
                .IgnoreNullValues(true);

            config.ForType<TrackLog, TrackLogDto>().IgnoreNullValues(true);

            config.ForType<ComponentLog, ComponentLogDto>().IgnoreNullValues(true);

            config.ForType<CargoSpace, CargoSpaceDto>()
                .Map(dest => dest.OrganizeCode, src => src.Organize!.Code)
                .Map(dest => dest.OrganizeName, src => src.Organize!.Name)
                .Map(dest => dest.ParentCode, src => src.Parent!.Code)
                .Map(dest => dest.ParentName, src => src.Parent!.Name)
                .Map(dest => dest.ChildrenDtos, src => src.Childrens)
                .Map(dest => dest.ComponentDtos, src => src.Components)
                .Map(dest => dest.ElementEntityDtos, src => src.ElementEntities)
                .IgnoreNullValues(true);

            config.ForType<AgvTaskSync, AgvTaskSyncDto>().IgnoreNullValues(true);

            return config;
        }
    }
}
