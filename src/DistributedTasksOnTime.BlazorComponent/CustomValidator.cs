using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent
{
    public class CustomValidator : ComponentBase
    {
        private ValidationMessageStore _messageStore;
        [CascadingParameter]
        public EditContext CurrentEditContext { get; set; }

        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException();
            }
            _messageStore = new(CurrentEditContext);
            CurrentEditContext.OnValidationRequested += (s, arg) => _messageStore.Clear();
        }

        public void DisplayErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                _messageStore.Add(CurrentEditContext.Field(error.Key), error.Value);
            }
            CurrentEditContext.NotifyValidationStateChanged();
        }
    }
}
