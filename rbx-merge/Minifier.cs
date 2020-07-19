using KopiLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static KopiLua.Lua;

namespace rbx_merge
{
    public class Minifier
    {
        public static string Minify(string source)
        {
            var L = lua_open();
            luaL_openlibs(L);

            int _minifyFuncRef;
            if (luaL_loadfile(L, "main.lua") == 0 && lua_pcall(L, 0, 1, 0) == 0)
            {
                _minifyFuncRef = luaL_ref(L, LUA_REGISTRYINDEX);
                lua_pushcfunction(L, LuaOutput);
                lua_setglobal(L, "to_output");
            }
            else
            {
                throw new Exception($"Minifier Error: {lua_tostring(L, -1)}");
            }

            var output = new StringBuilder();
            lua_rawgeti(L, LUA_REGISTRYINDEX, _minifyFuncRef);
            lua_pushstring(L, source);
            lua_pushlightuserdata(L, output);
            if (lua_pcall(L, 2, 0, 0) != 0)
            {
                throw new Exception($"Minifier Lua Error: {lua_tostring(L, -1)}");
            }

            return output.ToString();
        }

        private static int LuaOutput(lua_State L)
        {
            ((StringBuilder)lua_touserdata(L, 1)).Append(luaL_checkstring(L, 2));
            return 0;
        }
    }
}
