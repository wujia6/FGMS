select * from MenuInfos

--模块菜单
insert into MenuInfos(Client,[Name],Code) values
(1,'首页','home'),
(1,'系统设置','-'),
(1,'设备管理','-'),
(1,'砂轮管理','-'),
(1,'生产管理','-')

--系统设置
insert into MenuInfos(ParentId,Client,[Name],Code) values
(24,1,'组织管理','orginaze_management'),
(24,1,'角色管理','role_management'),
(24,1,'用户管理','user_management'),
(24,1,'菜单管理','menu_management')

--设备管理
insert into MenuInfos(ParentId,Client,[Name],Code) values(25,1,'机台管理','equipment_management')

--砂轮管理
insert into MenuInfos(ParentId,Client,[Name],Code) values
(26,1,'基础设置','-'),
(26,1,'仓储管理','-'),
(26,1,'工单管理','-')

insert into MenuInfos(ParentId,Client,[Name],Code) values
(33,1,'品牌管理','brand_management'),
(33,1,'料号管理','material_management'),
(33,1,'元件管理','element_management'),
(33,1,'标准组号管理','standard_number_management'),
(33,1,'标准组管理','standard_management'),
(33,1,'非标组管理','non_standard_management'),
(33,1,'标准组记录','standard_record'),
(33,1,'日志','log')

insert into MenuInfos(ParentId,Client,[Name],Code) values
(34,1,'货位管理','cargo_space_management'),
(34,1,'库存管理','inventory_management')

insert into MenuInfos(ParentId,Client,[Name],Code) values
(35,1,'砂轮工单分布','whell_order_distribution'),
(35,1,'砂轮工单','whell_order_management')

--生产管理
insert into MenuInfos(ParentId,Client,[Name],Code) values
(27,1,'制令单分布','production_order_distribution'),
(27,1,'制令单','production_order_management'),
(27,1,'机台变更','equipment_change_management'),
(27,1,'发料单','material_issue_order_management')