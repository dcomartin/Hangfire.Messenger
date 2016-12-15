using Hangfire.Common;
using Hangfire.States;

namespace Hangfire.Messenger.Internal
{
    internal class UseQueueFromParameterAttribute : JobFilterAttribute, IElectStateFilter
    {
        public UseQueueFromParameterAttribute(int parameterIndex)
        {
            ParameterIndex = parameterIndex;
        }

        private int ParameterIndex { get; set; }

        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                enqueuedState.Queue = context.BackgroundJob.Job.Args[ParameterIndex].ToString().Replace("\"", string.Empty);
            }
        }
    }
}
