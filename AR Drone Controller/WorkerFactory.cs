namespace AR_Drone_Controller
{
    internal class WorkerFactory
    {
        public virtual CommandWorker CreateCommandWorker(ISocketFactory socketFactory, ConnectParams connectArgs)
        {
            return null;
        }

        public virtual VideoWorker CreateVideoWorker(ISocketFactory socketFactory, ConnectParams connectArgs)
        {
            return null;
        }

        public virtual NavData.NavDataWorker CreateNavDataWorker(ISocketFactory socketFactory, ConnectParams connectArgs)
        {
            return null;
        }

        public virtual ControlWorker CreateControlWorker(ISocketFactory socketFactory, ConnectParams connectArgs)
        {
            return null;
        }
    }
}