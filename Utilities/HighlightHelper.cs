﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Utilities
{
    public static class HighlightHelper
    {
        public static Dictionary<string, string> Languages
        {
            get
            {
                return PrismLanguages; // Set to the currently used syntax list
            }
        }

        public static Dictionary<string, string> PrismLanguages
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "markup", "Markup" },
                    { "css", "CSS" },
                    { "clike", "C-like" },
                    { "javascript", "JavaScript" },
                    { "abap", "ABAP" },
                    { "actionscript", "ActionScript" },
                    { "ada", "Ada" },
                    { "apacheconf", "Apache Configuration" },
                    { "apl", "APL" },
                    { "applescript", "AppleScript" },
                    { "arduino", "Arduino" },
                    { "arff", "ARFF" },
                    { "asciidoc", "AsciiDoc" },
                    { "asm6502", "6502 Assembly" },
                    { "aspnet", "ASP.NET (C#)" },
                    { "autohotkey", "AutoHotkey" },
                    { "autoit", "AutoIt" },
                    { "bash", "Bash" },
                    { "basic", "BASIC" },
                    { "batch", "Batch" },
                    { "bison", "Bison" },
                    { "brainfuck", "Brainfuck" },
                    { "bro", "Bro" },
                    { "c", "C" },
                    { "csharp", "C#" },
                    { "cpp", "C++" },
                    { "coffeescript", "CoffeeScript" },
                    { "clojure", "Clojure" },
                    { "crystal", "Crystal" },
                    { "csp", "Content-Security-Policy" },
                    { "css-extras", "CSS Extras" },
                    { "d", "D" },
                    { "dart", "Dart" },
                    { "diff", "Diff" },
                    { "django", "Django/Jinja2" },
                    { "docker", "Docker" },
                    { "eiffel", "Eiffel" },
                    { "elixir", "Elixir" },
                    { "elm", "Elm" },
                    { "erb", "ERB" },
                    { "erlang", "Erlang" },
                    { "fsharp", "F#" },
                    { "flow", "Flow" },
                    { "fortran", "Fortran" },
                    { "gedcom", "GEDCOM" },
                    { "gherkin", "Gherkin" },
                    { "git", "Git" },
                    { "glsl", "GLSL" },
                    { "go", "Go" },
                    { "graphql", "GraphQL" },
                    { "groovy", "Groovy" },
                    { "haml", "Haml" },
                    { "handlebars", "Handlebars" },
                    { "haskell", "Haskell" },
                    { "haxe", "Haxe" },
                    { "http", "HTTP" },
                    { "hpkp", "HTTP Public-Key-Pins" },
                    { "hsts", "HTTP Strict-Transport-Security" },
                    { "ichigojam", "IchigoJam" },
                    { "icon", "Icon" },
                    { "inform7", "Inform 7" },
                    { "ini", "Ini" },
                    { "io", "Io" },
                    { "j", "J" },
                    { "java", "Java" },
                    { "jolie", "Jolie" },
                    { "json", "JSON" },
                    { "julia", "Julia" },
                    { "keyman", "Keyman" },
                    { "kotlin", "Kotlin" },
                    { "latex", "LaTeX" },
                    { "less", "Less" },
                    { "liquid", "Liquid" },
                    { "lisp", "Lisp" },
                    { "livescript", "LiveScript" },
                    { "lolcode", "LOLCODE" },
                    { "lua", "Lua" },
                    { "makefile", "Makefile" },
                    { "markdown", "Markdown" },
                    { "markup-templating", "Markup templating" },
                    { "matlab", "MATLAB" },
                    { "mel", "MEL" },
                    { "mizar", "Mizar" },
                    { "monkey", "Monkey" },
                    { "n4js", "N4JS" },
                    { "nasm", "NASM" },
                    { "nginx", "nginx" },
                    { "nim", "Nim" },
                    { "nix", "Nix" },
                    { "nsis", "NSIS" },
                    { "objectivec", "Objective-C" },
                    { "ocaml", "OCaml" },
                    { "opencl", "OpenCL" },
                    { "oz", "Oz" },
                    { "parigp", "PARI/GP" },
                    { "parser", "Parser" },
                    { "pascal", "Pascal" },
                    { "perl", "Perl" },
                    { "php", "PHP" },
                    { "php-extras", "PHP Extras" },
                    { "plsql", "PL/SQL" },
                    { "powershell", "PowerShell" },
                    { "processing", "Processing" },
                    { "prolog", "Prolog" },
                    { "properties", ".properties" },
                    { "protobuf", "Protocol Buffers" },
                    { "pug", "Pug" },
                    { "puppet", "Puppet" },
                    { "pure", "Pure" },
                    { "python", "Python" },
                    { "q", "Q (kdb+ database)" },
                    { "qore", "Qore" },
                    { "r", "R" },
                    { "jsx", "React JSX" },
                    { "tsx", "React TSX" },
                    { "renpy", "Ren'py" },
                    { "reason", "Reason" },
                    { "rest", "reST (reStructuredText)" },
                    { "rip", "Rip" },
                    { "roboconf", "Roboconf" },
                    { "ruby", "Ruby" },
                    { "rust", "Rust" },
                    { "sas", "SAS" },
                    { "sass", "Sass (Sass)" },
                    { "scss", "Sass (Scss)" },
                    { "scala", "Scala" },
                    { "scheme", "Scheme" },
                    { "smalltalk", "Smalltalk" },
                    { "smarty", "Smarty" },
                    { "sql", "SQL" },
                    { "soy", "Soy (Closure Template)" },
                    { "stylus", "Stylus" },
                    { "swift", "Swift" },
                    { "tap", "TAP" },
                    { "tcl", "Tcl" },
                    { "textile", "Textile" },
                    { "tt2", "Template Toolkit 2" },
                    { "twig", "Twig" },
                    { "typescript", "TypeScript" },
                    { "vbnet", "VB.Net" },
                    { "velocity", "Velocity" },
                    { "verilog", "Verilog" },
                    { "vhdl", "VHDL" },
                    { "vim", "vim" },
                    { "visual-basic", "Visual Basic" },
                    { "wasm", "WebAssembly" },
                    { "wiki", "Wiki markup" },
                    { "xeora", "Xeora" },
                    { "xojo", "Xojo (REALbasic)" },
                    { "xquery", "XQuery" },
                    { "yaml", "YAML" }
                };
            }
        }

        public static Dictionary<string, string> HighlightLanguages
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "1c", "1C" },
                    { "abnf", "ABNF" },
                    { "accesslog", "Access logs" },
                    { "ada", "Ada" },
                    { "armasm", "ARM assembler" },
                    { "arm", "ARM assembler" },
                    { "avrasm", "AVR assembler" },
                    { "actionscript", "ActionScript" },
                    { "as", "ActionScript" },
                    { "apache", "Apache" },
                    { "apacheconf", "Apache" },
                    { "applescript", "AppleScript" },
                    { "osascript", "AppleScript" },
                    { "asciidoc", "AsciiDoc" },
                    { "adoc", "AsciiDoc" },
                    { "aspectj", "AspectJ" },
                    { "autohotkey", "AutoHotkey" },
                    { "autoit", "AutoIt" },
                    { "awk", "Awk" },
                    { "mawk", "Awk" },
                    { "nawk", "Awk" },
                    { "gawk", "Awk" },
                    { "axapta", "Axapta" },
                    { "bash", "Bash" },
                    { "sh", "Bash" },
                    { "zsh", "Bash" },
                    { "basic", "Basic" },
                    { "bnf", "BNF" },
                    { "brainfuck", "Brainfuck" },
                    { "bf", "Brainfuck" },
                    { "cs", "C#" },
                    { "csharp", "C#" },
                    { "cpp", "C++" },
                    { "c", "C" },
                    { "cc", "C++" },
                    { "h", "C++" },
                    { "c++", "C++" },
                    { "h++", "C++" },
                    { "hpp", "C++" },
                    { "cal", "C/AL" },
                    { "cos", "Cache Object Script" },
                    { "cls", "Cache Object Script" },
                    { "cmake", "CMake" },
                    { "cmake.in", "CMake" },
                    { "coq", "Coq" },
                    { "csp", "CSP" },
                    { "css", "CSS" },
                    { "capnproto", "Cap’n Proto" },
                    { "capnp", "Cap’n Proto" },
                    { "clojure", "Clojure" },
                    { "clj", "Clojure" },
                    { "coffeescript", "CoffeeScript" },
                    { "coffee", "CoffeeScript" },
                    { "cson", "CoffeeScript" },
                    { "iced", "CoffeeScript" },
                    { "crmsh", "Crmsh" },
                    { "crm", "Crmsh" },
                    { "pcmk", "Crmsh" },
                    { "crystal", "Crystal" },
                    { "cr", "Crystal" },
                    { "d", "D" },
                    { "dns", "DNS Zone file" },
                    { "zone", "DNS Zone file" },
                    { "bind", "DNS Zone file" },
                    { "dos", "DOS" },
                    { "bat", "DOS" },
                    { "cmd", "DOS" },
                    { "dart", "Dart" },
                    { "delphi", "Delphi" },
                    { "dpr", "Delphi" },
                    { "dfm", "Delphi" },
                    { "pas", "Delphi" },
                    { "pascal", "Delphi" },
                    { "freepascal", "Delphi" },
                    { "lazarus", "Delphi" },
                    { "lpr", "Delphi" },
                    { "lfm", "Delphi" },
                    { "diff", "Diff" },
                    { "patch", "Diff" },
                    { "django", "Django" },
                    { "jinja", "Django" },
                    { "dockerfile", "Dockerfile" },
                    { "docker", "Dockerfile" },
                    { "dsconfig", "dsconfig" },
                    { "dts", "DTS (Device Tree)" },
                    { "dust", "Dust" },
                    { "dst", "Dust" },
                    { "ebnf", "EBNF" },
                    { "elixir", "Elixir" },
                    { "elm", "Elm" },
                    { "erlang", "Erlang" },
                    { "erl", "Erlang" },
                    { "excel", "Excel" },
                    { "xls", "Excel" },
                    { "xlsx", "Excel" },
                    { "fsharp", "F#" },
                    { "fs", "F#" },
                    { "fix", "FIX" },
                    { "fortran", "Fortran" },
                    { "f90", "Fortran" },
                    { "f95", "Fortran" },
                    { "gcode", "G-Code" },
                    { "nc", "G-Code" },
                    { "gams", "Gams" },
                    { "gms", "Gams" },
                    { "gauss", "GAUSS" },
                    { "gss", "GAUSS" },
                    { "gherkin", "Gherkin" },
                    { "go", "Go" },
                    { "golang", "Go" },
                    { "golo", "Golo" },
                    { "gololang", "Golo" },
                    { "gradle", "Gradle" },
                    { "groovy", "Groovy" },
                    { "xml", "XML" },
                    { "html", "HTML" },
                    { "xhtml", "XHTML" },
                    { "rss", "RSS" },
                    { "atom", "Atom" },
                    { "xjb", "HTML, XML" },
                    { "xsd", "HTML, XML" },
                    { "xsl", "HTML, XML" },
                    { "plist", "HTML, XML" },
                    { "http", "HTTP" },
                    { "https", "HTTP" },
                    { "haml", "Haml" },
                    { "handlebars", "Handlebars" },
                    { "hbs", "Handlebars" },
                    { "html.hbs", "Handlebars" },
                    { "html.handlebars", "Handlebars" },
                    { "haskell", "Haskell" },
                    { "hs", "Haskell" },
                    { "haxe", "Haxe" },
                    { "hx", "Haxe" },
                    { "hy", "Hy" },
                    { "hylang", "Hy" },
                    { "ini", "Ini" },
                    { "inform7", "Inform7" },
                    { "i7", "Inform7" },
                    { "irpf90", "IRPF90" },
                    { "json", "JSON" },
                    { "java", "Java" },
                    { "jsp", "Java" },
                    { "javascript", "JavaScript" },
                    { "js", "JavaScript" },
                    { "jsx", "JavaScript" },
                    { "leaf", "Leaf" },
                    { "lasso", "Lasso" },
                    { "lassoscript", "Lasso" },
                    { "less", "Less" },
                    { "ldif", "LDIF" },
                    { "lisp", "Lisp" },
                    { "livecodeserver", "LiveCode Server" },
                    { "livescript", "LiveScript" },
                    { "ls", "LiveScript" },
                    { "lua", "Lua" },
                    { "makefile", "Makefile" },
                    { "mk", "Makefile" },
                    { "mak", "Makefile" },
                    { "markdown", "Markdown" },
                    { "md", "Markdown" },
                    { "mkdown", "Markdown" },
                    { "mkd", "Markdown" },
                    { "mathematica", "Mathematica" },
                    { "mma", "Mathematica" },
                    { "matlab", "Matlab" },
                    { "maxima", "Maxima" },
                    { "mel", "Maya Embedded Language" },
                    { "mercury", "Mercury" },
                    { "mizar", "Mizar" },
                    { "mojolicious", "Mojolicious" },
                    { "monkey", "Monkey" },
                    { "moonscript", "Moonscript" },
                    { "moon", "Moonscript" },
                    { "n1ql", "N1QL" },
                    { "nsis", "NSIS" },
                    { "nginx", "Nginx" },
                    { "nginxconf", "Nginx" },
                    { "nimrod", "Nimrod" },
                    { "nim", "Nimrod" },
                    { "nix", "Nix" },
                    { "ocaml", "OCaml" },
                    { "ml", "OCaml" },
                    { "objectivec", "Objective C" },
                    { "mm", "Objective C" },
                    { "objc", "Objective C" },
                    { "obj-c", "Objective C" },
                    { "glsl", "OpenGL Shading Language" },
                    { "openscad", "OpenSCAD" },
                    { "scad", "OpenSCAD" },
                    { "ruleslanguage", "Oracle Rules Language" },
                    { "oxygene", "Oxygene" },
                    { "pf", "PF" },
                    { "pf.conf", "PF" },
                    { "php", "PHP" },
                    { "php3", "PHP" },
                    { "php4", "PHP" },
                    { "php5", "PHP" },
                    { "php6", "PHP" },
                    { "parser3", "Parser3" },
                    { "perl", "Perl" },
                    { "pl", "Perl" },
                    { "pm", "Perl" },
                    { "pony", "Pony" },
                    { "powershell", "PowerShell" },
                    { "ps", "PowerShell" },
                    { "processing", "Processing" },
                    { "prolog", "Prolog" },
                    { "protobuf", "Protocol Buffers" },
                    { "puppet", "Puppet" },
                    { "pp", "Puppet" },
                    { "python", "Python" },
                    { "py", "Python" },
                    { "gyp", "Python" },
                    { "profile", "Python profiler results" },
                    { "k", "Q" },
                    { "kdb", "Q" },
                    { "qml", "QML" },
                    { "r", "R" },
                    { "rib", "RenderMan RIB" },
                    { "rsl", "RenderMan RSL" },
                    { "graph", "Roboconf" },
                    { "instances", "Roboconf" },
                    { "ruby", "Ruby" },
                    { "rb", "Ruby" },
                    { "gemspec", "Ruby" },
                    { "podspec", "Ruby" },
                    { "thor", "Ruby" },
                    { "irb", "Ruby" },
                    { "rust", "Rust" },
                    { "rs", "Rust" },
                    { "scss", "SCSS" },
                    { "sql", "SQL" },
                    { "p21", "STEP Part 21" },
                    { "step", "STEP Part 21" },
                    { "stp", "STEP Part 21" },
                    { "scala", "Scala" },
                    { "scheme", "Scheme" },
                    { "scilab", "Scilab" },
                    { "sci", "Scilab" },
                    { "shell", "Shell" },
                    { "console", "Shell" },
                    { "smali", "Smali" },
                    { "smalltalk", "Smalltalk" },
                    { "st", "Smalltalk" },
                    { "stan", "Stan" },
                    { "stata", "Stata" },
                    { "stylus", "Stylus" },
                    { "styl", "Stylus" },
                    { "subunit", "SubUnit" },
                    { "swift", "Swift" },
                    { "tap", "Test Anything Protocol" },
                    { "tcl", "Tcl" },
                    { "tk", "Tcl" },
                    { "tex", "TeX" },
                    { "thrift", "Thrift" },
                    { "tp", "TP" },
                    { "twig", "Twig" },
                    { "craftcms", "Twig" },
                    { "typescript",  "TypeScript" },
                    { "ts", "TypeScript" },
                    { "vbnet",  "VB.Net" },
                    { "vb", "VB.Net" },
                    { "vbscript",  "VBScript" },
                    { "vbs", "VBScript" },
                    { "vhdl", "VHDL" },
                    { "vala", "Vala" },
                    { "verilog", "Verilog" },
                    { "v", "Verilog" },
                    { "vim", "Vim Script" },
                    { "x86asm", "x86 Assembly" },
                    { "xl", "XL" },
                    { "tao", "XL" },
                    { "xpath", "XQuery" },
                    { "xq", "XQuery" },
                    { "zephir", "Zephir" },
                    { "zep", "Zephir" }
                };
            }
        }
    }
}