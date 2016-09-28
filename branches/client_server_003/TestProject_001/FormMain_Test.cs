using uLoader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject_001
{
    
    
    /// <summary>
    ///Это класс теста для FormMain_Test, в котором должны
    ///находиться все модульные тесты FormMain_Test
    ///</summary>
    [TestClass()]
    public class FormMain_Test
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///Тест для FormMain_Load
        ///</summary>
        [DeploymentItem("uLoader.exe"), TestMethod()]
        public void FormMain_Load_Test()
        {
            FormMain_Accessor target = new FormMain_Accessor(); // TODO: инициализация подходящего значения
            object sender = null; // TODO: инициализация подходящего значения
            EventArgs e = null; // TODO: инициализация подходящего значения
            target.FormMain_Load(sender, e);
            //Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        }
    }
}
