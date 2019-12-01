namespace Database.Core.Utils
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    #endregion

    public static class AttributeExtensions
    {
        /// <summary>
        /// Obtiene un atributo.
        /// </summary>
        /// <typeparam name="TAttribute">El tipo de atributo</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// El atributo
        /// </returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            return enumValue.GetAttribute<TAttribute>(null).FirstOrDefault();

            //TAttribute attribute;

            //MemberInfo memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            //if (memberInfo == null)
            //{
            //    return null;
            //}

            //attribute = (TAttribute)memberInfo.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault();
            //return attribute;
        }

        /// <summary>
        /// Obtiene un atributo.
        /// </summary>
        /// <typeparam name="TAttribute">El tipo de atributo</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// El atributo
        /// </returns>
        public static List<TAttribute> GetAttribute<TAttribute>(this Enum enumValue, Func<TAttribute, bool> criteria = null) where TAttribute : Attribute
        {
            List<TAttribute> attributes;

            MemberInfo memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            if (memberInfo == null)
            {
                return null;
            }

            attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>().Where(a => criteria == null || criteria(a)).ToList();
            return attributes;
        }

        /// <summary>
        /// Obtiene un atributo.
        /// </summary>
        /// <typeparam name="TAttribute">El tipo de atributo</typeparam>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// El atributo
        /// </returns>
        public static TAttribute GetAttribute<TAttribute, TEnum>(this TEnum enumValue)
            where TAttribute : Attribute
            where TEnum : struct
        {
            TAttribute attribute;

            MemberInfo memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            if (memberInfo == null)
            {
                return null;
            }

            attribute = (TAttribute)memberInfo.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault();
            return attribute;
        }

        public static object GetAttributeValue<TAttribute>(this Enum enumeration, Func<TAttribute, object> expression)
            where TAttribute : Attribute
        {
            TAttribute attribute =
                enumeration
                    .GetType()
                    .GetMember(enumeration.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)?
                    .GetCustomAttributes(typeof(TAttribute), false)
                    .Cast<TAttribute>()
                    .SingleOrDefault();

            return attribute == null ? default(object) : expression(attribute);
        }
    }
}