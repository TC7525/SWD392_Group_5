using System.Collections.Generic;
using backend.Common;
using backend.Enums;

namespace backend.Services
{
    public interface IPOStatusService
    {
        bool CanTransition(POStatus current, POStatus target);
        void ValidateTransition(POStatus current, POStatus target);
    }

    public class POStatusService : IPOStatusService
    {
        private static readonly Dictionary<POStatus, HashSet<POStatus>> AllowedTransitions = new()
        {
            [POStatus.Pending] = new HashSet<POStatus>
            {
                POStatus.Approved,
                POStatus.Rejected
            },
            [POStatus.Approved] = new HashSet<POStatus>
            {
                POStatus.Processing
            },
            [POStatus.Processing] = new HashSet<POStatus>
            {
                POStatus.PartiallyReceived,
                POStatus.Completed,
                POStatus.Approved
            },
            [POStatus.PartiallyReceived] = new HashSet<POStatus>
            {
                POStatus.Processing
            },
            [POStatus.Completed] = new HashSet<POStatus>(),
            [POStatus.Rejected] = new HashSet<POStatus>()
        };

        public bool CanTransition(POStatus current, POStatus target)
        {
            return AllowedTransitions.TryGetValue(current, out var targets) && targets.Contains(target);
        }

        public void ValidateTransition(POStatus current, POStatus target)
        {
            if (!CanTransition(current, target))
            {
                throw new AppException("MSG16", "The requested status transition is not allowed.", 400);
            }
        }
    }
}
