using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Support
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class BooleanRequired : RequiredAttribute, IClientValidatable
    {

        public BooleanRequired()
        {

        }

        public override bool IsValid(object value)
        {
            return value != null && (bool)value == true;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRule() { ValidationType = "brequired", ErrorMessage = this.ErrorMessage } };
        }
    }
}