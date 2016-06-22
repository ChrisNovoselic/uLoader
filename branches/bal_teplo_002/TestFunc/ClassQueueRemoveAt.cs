using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TestFunc
{
    class ClassQueueRemoveAt
    {
        public ClassQueueRemoveAt()
        {
            Queue<int> _queueIdGroupSignals = new Queue<int> ();
            List<int> listIdGroupSignals = new List<int>(); // список идентификаторов групп сигналов - копия очереди для обработки (для исключения из очереди идентификатора останавливаемой группы)
            int id = -1; // идентификатор для удаления

            _queueIdGroupSignals.Enqueue(5);
            _queueIdGroupSignals.Enqueue(10);
            _queueIdGroupSignals.Enqueue(5);
            _queueIdGroupSignals.Enqueue(5);
            _queueIdGroupSignals.Enqueue(125);
            _queueIdGroupSignals.Enqueue(14);
            _queueIdGroupSignals.Enqueue(5);

            id = 5;

            Console.WriteLine(@"Всего элементов в очереди: {0}", _queueIdGroupSignals.Count);

            listIdGroupSignals = _queueIdGroupSignals.ToList<int>();
            _queueIdGroupSignals.Clear();
            while (listIdGroupSignals.Contains(id) == true)
                listIdGroupSignals.Remove(id);

            Console.WriteLine(@"Удалено ({0}): {1} шт.", id, _queueIdGroupSignals.Count () + listIdGroupSignals.Count);

            listIdGroupSignals.ForEach(delegate(int i1) { _queueIdGroupSignals.Enqueue(i1); });

            Console.WriteLine(@"Результат - очередь содержит искомых элементов ({0}): {1} шт.", id, _queueIdGroupSignals.Count(delegate(int i1) { return i1 == id; }));
        }
    }
}
