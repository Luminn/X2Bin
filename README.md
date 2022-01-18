# X2Bin
A XML to Binary tool optimized for .NET usage.

## Usage

    X2Bin --schema schema.yaml --input file/or/folder --output file.xbin [--mode xml|yaml|json]
        [--singleton] [--extension .thing] [--int 7bit|int8|int16|int32] [--float single|double] 
        [--string 7bit|int8|int16|int32|nullterm] [--encoding utf-8|utf-16|ascii...]
        [--trim-string aggressive|passive|none] [--trim-code cstyle|aggresive|passive|none]
        [--compression none|zlib]
        [--enum path/to/assembly.dll] [--csharpscript path/to/assembly.dll]
        
or

    X2Bin --build build.txt --output directory/to/dict

## Arguments
    --mode: type of file we scan, use xml|json to scan both, required in folder mode
    --input: if input is a folder, we will find all valid xml files in the folder, and read
             all root or level 1 child nodes that matches the root tag of the schema.
    --output: specify a folder, will generate code.xbin and dict.xbin if no localization
              was specified, otherwise several lang.xbin's will be generated instead of
              dict.xbin.
    --singleton only parse one object and omit count prefix
    --extension if specifyed as .a (or a), only search for files like .a.xml
    --int affects INT values, non-postfixed int literals, ENUMs and index of arrays
    --float affects non-postfixed floating point literals
    --encoding affects string encoding, could be a string or a codepage number, see .NET CodePagesEncodingProvider. 
    --string how strings lengths are encoded
             7bit|int8|int16|int32 defines the prefix length while
             nullterm means a c style '\0' terminated string with no length prefix
             
    --trim-string aggressive: trim leading and trailing space characters,
                              and trim all leading|trailing spaces in each line
                  passive: trim leading and trailing space characters
                  none: don't do anything
                  (disabled in xml property and json mode)
    --trim-code cstyle: remove all non-necessary whitespaces and newlines in c-like scripts
                        (Warning: currently may remove spaces in string literals)
    --enum if specifiyed, load all public enums in the .NET assembly.
           allows ENUM to parse .NET enums and write an INT value
    --csharpscript if specified, reads 
                        Object           [X2BGlobals]       as Globals
                        ScriptingOptions [X2BScriptOptions] as ScriptOptions
                   and pre-compile ALL SCRIPT|CODE as CSharpScript into assemblies


## Schema:
    TagName1: TYPE
    TagName2: [TYPE, default_value]
    TagName3: 
        ChildTag1: TYPE
        ChildTag2: TYPE
        ~Attribute1: TYPE
        $default: [value1, value2]

### Types:
### Concrete Types:
    BOOL ("", false, no, none are considered false, the rest are true), 1 byte
    BYTE|INT8, SHORT|INT16, INT32, LONG|INT64
    INT7 (7BitEncodedInt in C#, used for string length)
    FLOAT|SINGLE, DOUBLE
    STRING_TRIM, STRING_NOTRIM: saves string in the codex and writes index
    RAW_TRIM, RAW_NOTRIM: writes string directly
    ENUM: [ENUM, ClassName, OptionalDefaultStringEnumValue]
          parse to the INT value of a .NET Enum in an assembly

### Variable Types:
    INT: Serves as the indexer of arrays and values of Enums
         by default an INT7, could be int of any size
    STRING|RAW: Depends on the --trim-string setting
    TUPLE[X]: split the string by space into X segments, 
              writes ints as INTs and non-int strings as STRINGs
              (both uses INT encoding)
    CSV[X]: Tuple[X] but allows comma as a separator
    PAIR: same as TUPLE2

### Special Symbols:
    $any: an ARRAY of all child tags, writes count as an INT.
    $attrs: an ARRAY of all attributes as key-value pairs, writes count as an INT.
        note: $any, $attrs and ARRAYs will ignore already visited items
    $value: represents the text node
    $name: represents the name of the tag (when used with $any).
    $default: not a node, used for default value.
              if both $default and prefix? does not exist, a non-literal type missing will be counted as an error
    
    prefixes:
    ~: represents an XML attribute, in json mode or yaml mode will be treated a normal node
    $: specifies a recursive object (if not one of the reserved keywords)
    Example:
        $Tag:
            Children+:
                Child*: $Tag
    $$: non-root recursive object
    $$include: import recursive objects from schema
    
    postfixes:
    ?: Write a bool representing if the tag exists or not to the buffer,
       supresses the $default value check
    !: Remove default values on primitives, forcing an error
    *: Array, write count as an INT
    +: Array wrapper, if not exist, write count 0 as an INT
       Same as $default: 0
       Example:
           Things+:
               Thing*: STRING
    
### Value Types:
    1 INT 1b INT8 1s INT16 1i INT32 il INT64
    1.0 FLOAT/DOUBLE(if --float=double) 1f FLOAT 1d DOUBLE
    abc STRING
    true/false BOOL
    Array Syntax: [0b, 0b, abc]

