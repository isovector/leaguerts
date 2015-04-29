using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueEngine {
    /// <summary>
    /// Defines a UnitQueue which may dequeue elements from the middle and may peek at 
    /// objects in the middle.
    /// </summary>
    public class UnitQueue : Queue<string> {
        /// <summary>
        /// Dequeues a Uid at the specific location
        /// </summary>
        /// <param name="pos">The index of the Uid to remove</param>
        /// <returns>The dequeued Uid</returns>
        public string DequeueAt(int pos) {
            string ret = null;
            lock (this) {
                Queue<string> queue = new Queue<string>();
                int count = Count;

                for (int i = 0; i < count; i++) {
                    string u = Dequeue();
                    if (i != pos)
                        queue.Enqueue(u);
                    else
                        ret = u;
                }

                Clear();
                count = queue.Count;
                for (int i = 0; i < count; i++)
                    Enqueue(queue.Dequeue());
            }

            return ret;
        }

        /// <summary>
        /// Peeks at a Uid in the queue
        /// </summary>
        /// <param name="pos">The index at which to peek</param>
        /// <returns>The peeked Uid</returns>
        public string Peek(int pos) {
            return ToArray()[pos];
        }
    }
}
