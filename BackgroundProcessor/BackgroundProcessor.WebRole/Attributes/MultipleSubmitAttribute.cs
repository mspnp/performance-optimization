namespace BackgroundProcessor.WebRole.Attributes
{
    using System;
    using System.Reflection;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Method)]
    public class MultipleSubmitAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }

        public string Argument { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var isValidName = false;
            var keyValue = string.Format("{0}:{1}", this.Name, this.Argument);
            var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

            if (value != null)
            {
                controllerContext.Controller.ControllerContext.RouteData.Values[this.Name] = this.Argument;
                isValidName = true;
            }

            return isValidName;
        }
    }
}