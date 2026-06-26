import xlsx from 'node-xlsx';
import fs from 'fs';
import path from 'path';
import CsvParser from './CsvParser';

export default class DataParser {
    /**
     * Parse a data file (xlsx or CSV directory)
     * Returns data in the same format as node-xlsx.parse()
     */
    public static parse(filePath: string, format?: string): any[] {
        let data: any[] = [];
        
        if (format === 'csv') {
            if (CsvParser.canParseAsCsv(filePath)) {
                data = CsvParser.parse(filePath);
            }
        } else if (format === 'xlsx') {
            if (fs.existsSync(filePath)) {
                data = xlsx.parse(filePath);
            }
        } else {
            // Try CSV first, then fall back to xlsx (default behavior)
            if (CsvParser.canParseAsCsv(filePath)) {
                data = CsvParser.parse(filePath);
            }
            if (data.length === 0 && fs.existsSync(filePath)) {
                data = xlsx.parse(filePath);
            }
        }
        
        return data;
    }

    /**
     * Parse a data file with optional extension
     * First tries the path as-is, then tries with .xlsx extension
     */
    public static parseWithOptionalExtension(pathStr: string, format?: string): any[] {
        let data = this.parse(pathStr, format);
        
        // If not found, try with xlsx extension
        if (data.length === 0) {
            let parsedPath = path.parse(pathStr);
            parsedPath.base += '.xlsx';
            parsedPath.ext = '.xlsx';
            data = this.parse(path.format(parsedPath), format);
        }
        
        return data;
    }
}


