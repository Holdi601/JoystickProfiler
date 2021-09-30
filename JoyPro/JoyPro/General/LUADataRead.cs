using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public static class LUADataRead
    {
        public static string GetContentBetweenSymbols(string content, string openingSymbol, string closingSymbol = "")
        {
            if (content.Length < 1) return null;
            if (closingSymbol.Length < 1) closingSymbol = openingSymbol;
            if (openingSymbol.Length < 1) return null;
            string result = "";
            int srtindx = content.IndexOf(openingSymbol) + openingSymbol.Length;
            if (srtindx < 0) return null;
            if (!content.Contains(openingSymbol)) return null;
            if (openingSymbol == closingSymbol)
            {
                int closer = content.IndexOf(openingSymbol, srtindx);
                if (closer > 0)
                {
                    result = content.Substring(srtindx, closer - srtindx);
                }
                else
                {
                    result = content.Substring(srtindx);
                }
            }
            else
            {
                int level = 1;
                int initialopener = srtindx;
                int newOpener = srtindx;
                int closer = -1;
                while (level > 0)
                {
                    closer = content.IndexOf(closingSymbol, newOpener);
                    newOpener = content.IndexOf(openingSymbol, newOpener);
                    if (newOpener < closer && newOpener >= 0)
                    {
                        level++;
                        newOpener += openingSymbol.Length;
                    }
                    else
                    {
                        level -= 1;
                        newOpener = closer + closingSymbol.Length;
                    }
                    if (level > 1000000) { break; }
                }
                if ((newOpener - closingSymbol.Length - initialopener) >= 0)
                {
                    result = content.Substring(initialopener, newOpener - closingSymbol.Length - initialopener);
                }
                else
                {
                    result = content.Substring(initialopener);
                }
            }
            return result;
        }
        public static Dictionary<object, object> CreateAttributeDictFromLua(string cont)
        {
            Dictionary<object, object> result = new Dictionary<object, object>();
            if (cont.Length < 1) return null;
            string ltrim = cont.TrimStart();
            object key = null;
            int indxOfBracked = ltrim.IndexOf("[");
            string dtToCheck = ltrim.Substring(indxOfBracked + 1);
            LuaDataType ldtKey = DefineFirstDataTypeInString(dtToCheck);
            if (ldtKey == LuaDataType.String)
            {
                key = GetContentBetweenSymbols(ltrim, "\"");
            }
            else if (ldtKey == LuaDataType.Number)
            {
                key = Convert.ToInt32(GetContentBetweenSymbols(ltrim, "[", "]"));
            }
            else if (ldtKey == LuaDataType.Bool)
            {
                key = Convert.ToBoolean(GetContentBetweenSymbols(ltrim, "[", "]"));
            }
            while (key != null &&
                ((ldtKey == LuaDataType.String && ((string)key).Length > 0) ||
                (ldtKey == LuaDataType.Number && ((int)key) > -1)))
            {
                if (ldtKey == LuaDataType.String)
                {
                    int indexToStart = ltrim.IndexOf("\"" + (string)key + "\"");
                    ltrim = ltrim.Substring(indexToStart + ("\"" + (string)key + "\"").Length);
                }
                int equationInddex = ltrim.IndexOf("=");
                ltrim = ltrim.Substring(equationInddex + 1);
                LuaDataType ldtValue = DefineFirstDataTypeInString(ltrim);
                object val;
                int indxAfter = -1;
                switch (ldtValue)
                {
                    case LuaDataType.Dict:
                        string valRaw = GetContentBetweenSymbols(ltrim, "{", "}");
                        val = CreateAttributeDictFromLua(valRaw);
                        result.Add(key, val);
                        int ind = ltrim.IndexOf("{" + valRaw + "}");
                        indxAfter = ind + ("{" + valRaw + "}").Length;
                        break;
                    case LuaDataType.Number:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        val = Convert.ToDouble(ltrim.Substring(0, ltrim.IndexOf(",")), new CultureInfo("en-US"));
                        result.Add(key, val);
                        break;
                    case LuaDataType.String:
                        string valRw = GetContentBetweenSymbols(ltrim, "\"");
                        indxAfter = ltrim.IndexOf("\"" + valRw + "\"") + ("\"" + valRw + "\"").Length;
                        result.Add(key, valRw);
                        break;
                    case LuaDataType.Bool:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        val = Convert.ToBoolean(ltrim.Substring(0, ltrim.IndexOf(",")));
                        result.Add(key, val);
                        break;
                    case LuaDataType.Error:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        break;
                }
                ltrim = ltrim.Substring(indxAfter);
                indxOfBracked = ltrim.IndexOf("[");
                if (indxOfBracked < 0) break;
                dtToCheck = ltrim.Substring(indxOfBracked + 1);
                ldtKey = DefineFirstDataTypeInString(dtToCheck);
                if (ldtKey == LuaDataType.String)
                {
                    key = GetContentBetweenSymbols(ltrim, "\"");
                }
                else if (ldtKey == LuaDataType.Number)
                {
                    key = Convert.ToInt32(GetContentBetweenSymbols(ltrim, "[", "]"));
                }
                else if (ldtKey == LuaDataType.Bool)
                {
                    key = Convert.ToBoolean(GetContentBetweenSymbols(ltrim, "[", "]"));
                }
            }
            return result;
        }
        public static LuaDataType DefineFirstDataTypeInString(string cont)
        {
            if (cont.Length < 1) return LuaDataType.Error;
            int indxQuotas = cont.IndexOf("\"");
            int indxCurlyBrackets = cont.IndexOf("{");
            int indxBool = cont.IndexOf("true");
            if ((cont.IndexOf("false") > -1 && cont.IndexOf("false") < indxBool) || indxBool < 0)
                indxBool = cont.IndexOf("false");
            int indxNumber = int.MaxValue;
            for (int i = -1; i < 10; ++i)
            {
                int tempIndex = cont.IndexOf(i.ToString().Substring(0, 1));
                if (tempIndex > -1 && tempIndex < indxNumber)
                    indxNumber = tempIndex;
            }
            if (indxNumber == int.MaxValue) indxNumber = -1;
            if (IsFirstValueLowestButNotNegative(indxQuotas, indxCurlyBrackets, indxBool, indxNumber)) return LuaDataType.String;
            if (IsFirstValueLowestButNotNegative(indxCurlyBrackets, indxQuotas, indxBool, indxNumber)) return LuaDataType.Dict;
            if (IsFirstValueLowestButNotNegative(indxNumber, indxQuotas, indxCurlyBrackets, indxBool)) return LuaDataType.Number;
            if (IsFirstValueLowestButNotNegative(indxBool, indxQuotas, indxCurlyBrackets, indxNumber)) return LuaDataType.Bool;
            return LuaDataType.Error;
        }
        public static bool IsFirstValueLowestButNotNegative(int val1, int val2, int val3, int val4)
        {
            if (val1 < 0) return false;
            List<int> toCheck = new List<int>();
            toCheck.Add(val1);
            if (val2 > -1) toCheck.Add(val2);
            if (val3 > -1) toCheck.Add(val3);
            if (val4 > -1) toCheck.Add(val4);
            for (int i = 1; i < toCheck.Count; ++i)
                if (toCheck[0] > toCheck[i]) return false;
            return true;
        } 
    }
}
