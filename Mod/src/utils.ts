export default class Utils {

    public static GetFristUpperAndLowerStr(str: string): string[] {
        let upper = str.substring(0,1).toUpperCase()+str.substring(1);
        let lower = str.substring(0,1).toLowerCase()+str.substring(1);
        return [upper, lower];
    }

    public static FormatStr(str:string, ...args: string[]): string {
        let result = str;
        for(let i = 0; i < args.length; ++i){
            result = result.replace("{" + i + "}", args[i]);
        }
        return result;
    }

}