using System;
using System.Threading;

namespace PFShop
{
    public delegate void TimeOutEventHandler();

    public interface ITimeOutThread
    {
        event TimeOutEventHandler DataTimeOutEvent;   // 超时事件
        void StopTimeOutCheck(); // 暂停超时检测
        void StartTimeOutCheck();  // 恢复超时检测
        void ClearTimeOutMark();   // 清除超时标志，防止超时
        void DisposeTimeOutCheck();  // 彻底关闭超时检测 
        int TimeOutInterval   // 超时间隔，单位为ms
        {
            get;
            set;
        } 
    }

    /// <summary>
    /// 超时检测线程
    /// 运行方式：
    /// 1. 设置超时时间，默认超时时间为 200ms
    /// 2. 通过ResumeTimeOutCheck()来启动超时检测, SuspendTimeOutCheck()来暂停超时检测
    /// 3. 启动超时检测后，通过ClearTimeOutMark()来不断清除超时检测，防止其超时，通过SetTimeOutMark()来触发超时事件
    /// 4. 超时事件为DataTimeOutEvent， 通过绑定超时事件处理函数来处理超时事件（超时事件发出后不暂停超时检测，这意味这需要手动暂停）
    /// 5. 在线程使用完毕后一定要停止超时，停止后超时检测将直接停止（不可恢复）
    /// </summary>
    class TimeOutThread : ITimeOutThread
    {
        enum m_ThreadState
        {
            Stopped = 0,
            Started,
            Suspended,
            Resumed,
        };

        private int timeOutMark;   // 超时标志
        private m_ThreadState stateOfThread;   // 线程运行状态
        private Thread checkMarkThread;        // 检测超时线程
        private object criticalAraeLockItem;   // 线程安全锁变量
        private bool threadControl;            // 线程停止循环运行标志

        private ManualResetEvent manualResetEvent;   // 线程控制事件， 当被手动Reset()则WaitOne()会使线程阻塞
                                                     // 当被Set()便再不会被阻塞直到Reset()
                                                     // 在本类中，当停止检测超时时便手动Reset()使线程阻塞，开始检测
                                                     // 时Set()以使线程持续执行

        private int timeOutInterval;    // 超时时间
        public int TimeOutInterval
        {
            get
            {
                return timeOutInterval;
            }

            set
            {
                timeOutInterval = value;
            }
        }

        public event TimeOutEventHandler DataTimeOutEvent;

        /// <summary>
        /// 构造函数， 通过设置超时时间初始化
        /// </summary>
        /// <param name="timeOutTime"></param>
        public TimeOutThread(int timeoutTime)
        {
            this.criticalAraeLockItem = new object();
            this.threadControl = true;
            this.timeOutInterval = timeoutTime;
            this.ClearTimeOutMark();
            this.stateOfThread = m_ThreadState.Suspended;
            this.checkMarkThread = new Thread(new ThreadStart(this.CheckTimeOutMark));
            this.manualResetEvent = new ManualResetEvent(false);  // 初始情况便阻塞以便启动线程
            this.checkMarkThread.Start();  // 此时虽然启动线程，但线程阻塞，不会运行
        }

        /// <summary>
        ///  默认构造函数，默认超时200ms
        /// </summary>
        public TimeOutThread()
            : this(200)
        {

        }

        /// <summary>
        /// 阻塞线程
        /// </summary>
        private void SuspendThread()
        {
            this.manualResetEvent.Reset();
        }

        /// <summary>
        /// 恢复线程
        /// </summary>
        private void ResumeThread()
        {
            this.manualResetEvent.Set();
        }

        /// <summary>
        /// 启动超时检测线程
        /// </summary>
        public void StartTimeOutCheck()
        {
            if (this.stateOfThread == m_ThreadState.Suspended) // 线程已启动但是被挂起
            {
                // 恢复线程
                this.ResumeThread();
            }

            // 更新状态
            this.stateOfThread = m_ThreadState.Resumed;
        }

        /// <summary>
        /// 停止超时检测线程
        /// </summary>
        public void StopTimeOutCheck()
        {
            if (this.stateOfThread == m_ThreadState.Resumed)
            {
                this.SuspendThread();
                this.stateOfThread = m_ThreadState.Suspended;
            }
        }

        /// <summary>
        /// 彻底停止超时检测
        /// </summary>
        public void DisposeTimeOutCheck()
        {
            this.threadControl = false;  // 停止线程循环
            this.ResumeThread();
            this.stateOfThread = m_ThreadState.Suspended;

            try
            {
                this.checkMarkThread.Abort();
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 检测超时标记是否已经被清除
        /// </summary>
        /// <returns></returns>
        private bool IsTimeOutMarkCleared()
        {
            if (this.timeOutMark == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 清除超时标记
        /// </summary>
        public void ClearTimeOutMark()
        {
            lock (this.criticalAraeLockItem)
            {
                this.timeOutMark = 1;  // 清除超时标记
            }
        }


        /// <summary>
        /// 设置超时标记
        /// </summary>
        private void SetTimeOutMark()
        {
            lock (this.criticalAraeLockItem)
            {
                this.timeOutMark = 0;  // 设置超时标记
            }
        }

        /// <summary>
        /// routine work， 在threadControl不为false时不断检测timeOutMark，若有超时，则发出超时事件
        /// </summary>
        private void CheckTimeOutMark()
        {
            while (this.threadControl == true)
            {
                manualResetEvent.WaitOne();  // 用以阻塞线程, 当Set()被调用后恢复，Reset()被调用后阻塞

                Thread.Sleep(this.timeOutInterval);  // 线程睡眠超时事件长度

                if (this.IsTimeOutMarkCleared()) // 线程超时标志已被更新，不发出超时事件
                {
                    //设置超时标志， 若下次检测超时标记依旧处于被设置状态，则超时
                    this.SetTimeOutMark();
                }
                else
                {
                    // 超时标志未被清除并且未被要求停止检测超时，发出超时事件
                    if ((DataTimeOutEvent != null) && (this.stateOfThread == m_ThreadState.Resumed))
                    {
                        DataTimeOutEvent.Invoke();
                    }
                }
            }

        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TimeOutThread()
        {
            this.DisposeTimeOutCheck(); // 彻底停止线程
        }
    }
}
