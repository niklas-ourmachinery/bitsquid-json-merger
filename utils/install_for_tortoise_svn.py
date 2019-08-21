import _winreg

SJSON_FILE_FORMATS = ["animation", "animation_set", "bones", "bsi", "config",
	"editor", "flow", "flow_editor", "font", "ini", "level", "level_editor",
	"material", "package", "particle_editor", "particles", "physics", "physics_properties",
	"script_flow_nodes", "shader_source", "shaders", "shading_environment",
	"shading_environment_template", "state_machine", "statemachine_editor",
	"texture", "unit", "sjson"]
	
JSON_FILE_FORMATS = ["json"]

MERGER_PATH = "C:\\Program Files\\Json Merge\\json_merge.exe"

tortoise_svn = _winreg.CreateKey(_winreg.HKEY_CURRENT_USER, "Software\\TortoiseSVN")
difftools = _winreg.CreateKey(tortoise_svn, "DiffTools")
mergetools = _winreg.CreateKey(tortoise_svn, "MergeTools")

for ff in SJSON_FILE_FORMATS:
	_winreg.SetValueEx(difftools, "." + ff, 0, _winreg.REG_SZ, '"' + MERGER_PATH + '"' + " -window -diff %base %mine")
	_winreg.SetValueEx(mergetools, "." + ff, 0, _winreg.REG_SZ, '"' + MERGER_PATH + '"' + " -window -merge %base %theirs %mine %merged")
	
for ff in JSON_FILE_FORMATS:
	_winreg.SetValueEx(difftools, "." + ff, 0, _winreg.REG_SZ, '"' + MERGER_PATH + '"' + " -json -window -diff %base %mine")
	_winreg.SetValueEx(mergetools, "." + ff, 0, _winreg.REG_SZ, '"' + MERGER_PATH + '"' + " -json -window -merge %base %theirs %mine %merged")
