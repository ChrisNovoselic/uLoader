﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      Этот код был создан построителем кодированных тестов ИП.
//      Версия: 10.0.0.0
//
//      Изменения, внесенные в этот файл, могут привести к неправильной работе кода и будут
//      утрачены при повторном формировании кода.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace TestProject_001
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public partial class UIMap
    {
        
        /// <summary>
        /// AssertMethod1 - Используйте "AssertMethod1ExpectedValues" для передачи параметров в этот метод.
        /// </summary>
        public void AssertMethod1()
        {
            #region Variable Declarations
            WinClient uIAdornerWindowClient = this.UIULoaderMicrosoftVisuWindow.UIItemTabList.UIFormAboutcsКонструктTabPage.UIFormAboutcsКонструктPane.UIFormAboutcsКонструктPane1.UIOverlayControlClient.UIAdornerWindowClient;
            #endregion

            // Убедитесь, что "AdornerWindow" клиент свойство "ControlType" равно "Client"
            Assert.AreEqual(this.AssertMethod1ExpectedValues.UIAdornerWindowClientControlType, uIAdornerWindowClient.ControlType.ToString());

            // Убедитесь, что "AdornerWindow" клиент свойство "ControlType" равно "Client"
            Assert.AreEqual(this.AssertMethod1ExpectedValues.UIAdornerWindowClientControlType1, uIAdornerWindowClient.ControlType.ToString());
        }
        
        #region Properties
        public virtual AssertMethod1ExpectedValues AssertMethod1ExpectedValues
        {
            get
            {
                if ((this.mAssertMethod1ExpectedValues == null))
                {
                    this.mAssertMethod1ExpectedValues = new AssertMethod1ExpectedValues();
                }
                return this.mAssertMethod1ExpectedValues;
            }
        }
        
        public UIULoaderMicrosoftVisuWindow UIULoaderMicrosoftVisuWindow
        {
            get
            {
                if ((this.mUIULoaderMicrosoftVisuWindow == null))
                {
                    this.mUIULoaderMicrosoftVisuWindow = new UIULoaderMicrosoftVisuWindow();
                }
                return this.mUIULoaderMicrosoftVisuWindow;
            }
        }
        #endregion
        
        #region Fields
        private AssertMethod1ExpectedValues mAssertMethod1ExpectedValues;
        
        private UIULoaderMicrosoftVisuWindow mUIULoaderMicrosoftVisuWindow;
        #endregion
    }
    
    /// <summary>
    /// Параметры для передачи в "AssertMethod1"
    /// </summary>
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class AssertMethod1ExpectedValues
    {
        
        #region Fields
        /// <summary>
        /// Убедитесь, что "AdornerWindow" клиент свойство "ControlType" равно "Client"
        /// </summary>
        public string UIAdornerWindowClientControlType = "Client";
        
        /// <summary>
        /// Убедитесь, что "AdornerWindow" клиент свойство "ControlType" равно "Client"
        /// </summary>
        public string UIAdornerWindowClientControlType1 = "Client";
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIULoaderMicrosoftVisuWindow : WinWindow
    {
        
        public UIULoaderMicrosoftVisuWindow()
        {
            #region Условия поиска
            this.SearchProperties[WinWindow.PropertyNames.Name] = "uLoader - Microsoft Visual Studio (Администратор)";
            this.SearchProperties.Add(new PropertyExpression(WinWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public UIItemTabList UIItemTabList
        {
            get
            {
                if ((this.mUIItemTabList == null))
                {
                    this.mUIItemTabList = new UIItemTabList(this);
                }
                return this.mUIItemTabList;
            }
        }
        #endregion
        
        #region Fields
        private UIItemTabList mUIItemTabList;
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIItemTabList : WpfTabList
    {
        
        public UIItemTabList(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Условия поиска
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public UIFormAboutcsКонструктTabPage UIFormAboutcsКонструктTabPage
        {
            get
            {
                if ((this.mUIFormAboutcsКонструктTabPage == null))
                {
                    this.mUIFormAboutcsКонструктTabPage = new UIFormAboutcsКонструктTabPage(this);
                }
                return this.mUIFormAboutcsКонструктTabPage;
            }
        }
        #endregion
        
        #region Fields
        private UIFormAboutcsКонструктTabPage mUIFormAboutcsКонструктTabPage;
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIFormAboutcsКонструктTabPage : WpfTabPage
    {
        
        public UIFormAboutcsКонструктTabPage(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Условия поиска
            this.SearchProperties[WpfTabPage.PropertyNames.Name] = "FormAbout.cs [Конструктор]";
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public UIFormAboutcsКонструктPane UIFormAboutcsКонструктPane
        {
            get
            {
                if ((this.mUIFormAboutcsКонструктPane == null))
                {
                    this.mUIFormAboutcsКонструктPane = new UIFormAboutcsКонструктPane(this);
                }
                return this.mUIFormAboutcsКонструктPane;
            }
        }
        #endregion
        
        #region Fields
        private UIFormAboutcsКонструктPane mUIFormAboutcsКонструктPane;
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIFormAboutcsКонструктPane : WpfPane
    {
        
        public UIFormAboutcsКонструктPane(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Условия поиска
            this.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.ViewPresenter";
            this.SearchProperties[WpfPane.PropertyNames.AutomationId] = "D:0:0:{33C90FD1-85FE-41F0-84AD-712E89AD8F2D}|uLoader\\uLoader.csproj|d:\\my project" +
                "\'s\\work\'s\\c.net\\uloader\\uloader\\formabout.cs||{A6C744A8-0E4A-4FC6-886A-064283054" +
                "674}|Form";
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public UIFormAboutcsКонструктPane1 UIFormAboutcsКонструктPane1
        {
            get
            {
                if ((this.mUIFormAboutcsКонструктPane1 == null))
                {
                    this.mUIFormAboutcsКонструктPane1 = new UIFormAboutcsКонструктPane1(this);
                }
                return this.mUIFormAboutcsКонструктPane1;
            }
        }
        #endregion
        
        #region Fields
        private UIFormAboutcsКонструктPane1 mUIFormAboutcsКонструктPane1;
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIFormAboutcsКонструктPane1 : WpfPane
    {
        
        public UIFormAboutcsКонструктPane1(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Условия поиска
            this.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.GenericPane";
            this.SearchProperties[WpfPane.PropertyNames.Name] = "FormAbout.cs [Конструктор]";
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public UIOverlayControlClient UIOverlayControlClient
        {
            get
            {
                if ((this.mUIOverlayControlClient == null))
                {
                    this.mUIOverlayControlClient = new UIOverlayControlClient(this);
                }
                return this.mUIOverlayControlClient;
            }
        }
        #endregion
        
        #region Fields
        private UIOverlayControlClient mUIOverlayControlClient;
        #endregion
    }
    
    [GeneratedCode("Построитель кодированных тестов ИП", "10.0.40219.457")]
    public class UIOverlayControlClient : WinClient
    {
        
        public UIOverlayControlClient(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Условия поиска
            this.SearchProperties[WinControl.PropertyNames.Name] = "OverlayControl";
            this.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
            #endregion
        }
        
        #region Properties
        public WinClient UIAdornerWindowClient
        {
            get
            {
                if ((this.mUIAdornerWindowClient == null))
                {
                    this.mUIAdornerWindowClient = new WinClient(this);
                    #region Условия поиска
                    this.mUIAdornerWindowClient.SearchProperties[WinControl.PropertyNames.Name] = "AdornerWindow";
                    this.mUIAdornerWindowClient.WindowTitles.Add("uLoader - Microsoft Visual Studio (Администратор)");
                    #endregion
                }
                return this.mUIAdornerWindowClient;
            }
        }
        #endregion
        
        #region Fields
        private WinClient mUIAdornerWindowClient;
        #endregion
    }
}