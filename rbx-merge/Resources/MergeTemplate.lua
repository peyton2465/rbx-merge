do
	local imports = {0}

	local o_require = require
	setmetatable(imports, {{
		__index = function(self, k) 
			return function() 
				return o_require(k) 
			end
		end
	}})
	
	function require(source)
		return imports[source]()
	end
end

{1}