using Hangfire.Common;
using Hangfire.States;

namespace Hangfire.Message
{
    internal class UseQueueFromParameterAttribute : JobFilterAttribute, IElectStateFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueAttribute"/> class
        /// using the specified queue name.
        /// </summary>
        /// <param name="queue">Queue name.</param>
        public UseQueueFromParameterAttribute(int parameterIndex)
        {
            this.ParameterIndex = parameterIndex;
        }

        public int ParameterIndex { get; private set; }

        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                enqueuedState.Queue = context.Job.Arguments[ParameterIndex].Replace("\"", string.Empty);
            }
        }
    }
}
