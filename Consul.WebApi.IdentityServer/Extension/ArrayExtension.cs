using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Extension
{
    public static class ArrayExtension
    {
        #region 01,arrays of int type are split into strings based on separator+static string ToString(this int[] array, char[] splitSymbol)
        /// <summary>
        /// arrays of int type are split into strings based on separator
        /// </summary>
        /// <param name="array">arrays of int</param>
        /// <param name="splitSymbol">separator</param>
        /// <returns></returns>
        public static string ArrayToString(this int[] array, char[] splitSymbol)
        {
            var str = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                str += array[i].ToString() + splitSymbol[0];
            }
            return str.Substring(0, str.Length - 1);
        }
        #endregion

        #region 02,arrays of string type are split into strings based on separator+static string ToString(this string[] array, char[] splitSymbol)
        /// <summary>
        /// arrays of string type are split into strings based on separator
        /// </summary>
        /// <param name="array">arrays of string</param>
        /// <param name="splitSymbol">separator</param>
        /// <returns></returns>
        public static string ArrayToString(this string[] array, char[] splitSymbol)
        {
            var str = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                str += array[i] + splitSymbol[0];
            }
            return str.Substring(0, str.Length - 1);
        }
        #endregion
    }
}
