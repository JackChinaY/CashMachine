using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MoreToOne
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //log4net输出日志用
        //public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public App()
        {
        }
        /// <summary>
        /// log4net输出日志用 开始,如果要使用日志功能，就将下面两个函数启用
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    log4net.Config.XmlConfigurator.Configure();
        //    base.OnStartup(e);
        //    log.Info("==Startup=====================>>>");
        //}
        /// <summary>
        /// log4net输出日志用 结束
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnExit(ExitEventArgs e)
        //{
        //    log.Info("<<<========================End==");
        //    base.OnExit(e);
        //}
    }
}
