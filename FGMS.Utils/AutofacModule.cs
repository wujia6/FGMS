using Autofac;
using FGMS.Core.EfCore.Implements;
using FGMS.Core.EfCore.Interfaces;

namespace FGMS.Utils
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                //注册数据库上下文服务
                builder.RegisterType<FgmsDbContext>().As<IFgmsDbContext>().InstancePerLifetimeScope();

                //注册泛型仓储服务
                builder.RegisterGeneric(typeof(FgmsDbRepository<>)).As(typeof(IFgmsDbRepository<>)).InstancePerLifetimeScope();

                //注册实体仓储服务
                builder.RegisterAssemblyTypes(ApplicationFactory.GetAssembly("FGMS.Repositories"))
                    .Where(tp => tp.Name.EndsWith("Repository") && !tp.IsInterface && !tp.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                //注册实体应用服务
                builder.RegisterAssemblyTypes(ApplicationFactory.GetAssembly("FGMS.Services"))
                    .Where(tp => tp.Name.EndsWith("Service") && !tp.IsInterface && !tp.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
            catch (Exception)
            {
                throw;
            }
            base.Load(builder);
        }
    }
}
