using System.Collections.Generic;
using System.Linq;

namespace Seculus.MobileScript.Core.Helpers
{
    public class ValidatorHelper
    {
        public static bool Validate<T>(IValidator<T> validator, T entity, out IList<string> violatedRules)
        {
            Check.Argument.IsNotNull(validator, "Validator cannot be null.");
            Check.Argument.IsNotNull(entity, "Entity cannot be null");

            violatedRules = validator.ViolatedRules(entity);

            return !violatedRules.Any();            
        }
    }
}
