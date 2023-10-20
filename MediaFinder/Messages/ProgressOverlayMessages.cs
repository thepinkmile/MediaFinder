using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaFinder_v2.Messages
{
    public record ShowProgressMessage(int Progress = 0, ICommand? CancelCommand = null, string? Message = null)
    {
        public static ShowProgressMessage Create(string message, ICommand? cancelCommanad = null)
            => Create(0, message, cancelCommanad);
        public static ShowProgressMessage Create(int progress, ICommand? cancelCommanad = null)
            => new(progress, cancelCommanad);

        public static ShowProgressMessage Create(int progress, string message, ICommand? cancelCommanad = null)
            => new(progress, cancelCommanad, message);
    }
}
