using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace RifaDeliverySystem.Web.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Devuelve el texto definido en [Display(Name = …)] para un valor de enum.
        /// Si no hay atributo, devuelve value.ToString().
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name is null) return value.ToString();

            var field = type.GetField(name);
            if (field is null) return value.ToString();

            var attr = field.GetCustomAttribute<DisplayAttribute>();
            return attr?.GetName() ?? value.ToString();
        }
    }
}
