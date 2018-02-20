using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFunc
{
    class HandlerUnRegister
    {
        private class A
        {
            public event Action Event;

            public void PerformEvent ()
            {
                Event?.Invoke ();
            }

            public virtual void UnRegister (Type type)
            {
                //TODO:
            }
        }

        private class AA : A
        {
            public override void UnRegister (Type type)
            {
                //TODO:
            }
        }

        private class B
        {
            public void OnEvent ()
            {
            }
        }

        private class C
        {
            public void OnEvent ()
            {
                throw new Exception ("Задача не выполнена...");
            }
        }

        public HandlerUnRegister ()
        {
            A a = new A();
            AA aa = new AA ();

            B b = new B ();
            C c = new C ();

            a.Event += b.OnEvent;
            a.Event += c.OnEvent;

            a.UnRegister (c.GetType());

            a.PerformEvent ();
        }
    }
}