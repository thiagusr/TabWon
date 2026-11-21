import importlib.util

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
exe = EXE(pyz, a.scripts, [], exclude_binaries=True, name="TabWon", debug=False,
          bootloader_ignore_signals=False, strip=False, upx=False, console=False,
          icon="TabWon_configs\\TabWon_icos.ico",
          contents_directory="TabWon_dependencias")
coll = COLLECT(exe, a.binaries, a.datas, strip=False, upx=False, name="TabWon")
