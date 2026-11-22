import importlib.util
import re
from PyInstaller.utils.win32.versioninfo import (VSVersionInfo, FixedFileInfo, StringFileInfo, StringTable,
                                                 StringStruct, VarFileInfo, VarStruct)

with open("TabWon.py", "r", encoding="utf-8") as _f:
    versao = re.search(r'^VERSAO\s*=\s*"([^"]+)"', _f.read(), re.M).group(1)
numeros = tuple(int(n) for n in (versao.split(".") + ["0", "0", "0", "0"])[:4])
info_versao = VSVersionInfo(
    ffi=FixedFileInfo(filevers=numeros, prodvers=numeros, mask=0x3F, flags=0x0, OS=0x40004, fileType=0x1,
                      subtype=0x0, date=(0, 0)),
    kids=[StringFileInfo([StringTable("041604B0", [
              StringStruct("CompanyName", "Thiago Rodrigues"),
              StringStruct("FileDescription", "TabWon - Tabulador Livre de dados do SUS"),
              StringStruct("FileVersion", versao),
              StringStruct("InternalName", "TabWon_tabulador"),
              StringStruct("LegalCopyright", "Copyright (C) 2026 Thiago Rodrigues - GNU GPL v3"),
              StringStruct("OriginalFilename", "TabWon_tabulador.exe"),
              StringStruct("ProductName", "TabWon"),
              StringStruct("ProductVersion", versao)])]),
          VarFileInfo([VarStruct("Translation", [0x0416, 1200])])])

ocultos = ["pyarrow.parquet", "pyarrow.dataset", "duckdb", "openpyxl", "PIL.ImageTk",
           "matplotlib.backends.backend_tkagg", "win32clipboard"]
ocultos += [m for m in ("climasus_readdbc", "climasus_readdbc_py", "readdbc", "pyreaddbc")
            if importlib.util.find_spec(m)]

excluidos = ["PyQt5", "PyQt6", "PySide2", "PySide6", "wx", "gi", "IPython", "jupyter", "notebook",
             "scipy", "sklearn", "pytest", "sphinx", "docutils", "setuptools", "pip",
             "matplotlib.backends.backend_qt", "matplotlib.backends.backend_qtagg",
             "matplotlib.backends.backend_qt5agg", "matplotlib.backends.backend_gtk3agg",
             "matplotlib.backends.backend_wxagg", "matplotlib.backends.backend_webagg",
             "pandas.tests", "numpy.tests", "pyarrow.tests", "tkinter.test"]

a = Analysis(["TabWon.py"], pathex=[], binaries=[],
             datas=[("TabWon_configs\\TabWon_icos.ico", "TabWon_configs")], hiddenimports=ocultos,
             hookspath=[], runtime_hooks=[], excludes=excluidos, noarchive=False, optimize=1)
pyz = PYZ(a.pure)
exe = EXE(pyz, a.scripts, [], exclude_binaries=True, name="TabWon_tabulador", debug=False,
          bootloader_ignore_signals=False, strip=False, upx=False, console=False,
          icon="TabWon_configs\\TabWon_icos.ico", version=info_versao,
          contents_directory="TabWon_dependencias")
coll = COLLECT(exe, a.binaries, a.datas, strip=False, upx=False, name="TabWon")
