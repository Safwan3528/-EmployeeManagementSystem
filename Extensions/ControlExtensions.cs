namespace EmployeeManagementSystem.Extensions
{
    public static class ControlExtensions
    {
        public static IEnumerable<Control> GetAllControls(this Control control)
        {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => ctrl.GetAllControls())
                .Concat(controls);
        }
    }
} 