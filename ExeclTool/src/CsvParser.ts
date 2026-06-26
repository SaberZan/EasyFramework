import fs from 'fs';
import path from 'path';

export interface CsvSheetData {
    name: string;
    data: any[][];
}

export default class CsvParser {
    /**
     * Parse a CSV string into a 2D array
     */
    public static parseCsv(csvContent: string): any[][] {
        const lines: string[] = [];
        let currentLine = '';
        let inQuotes = false;
        
        for (let i = 0; i < csvContent.length; i++) {
            const char = csvContent[i];
            
            if (char === '\x22') {
                if (inQuotes && csvContent[i + 1] === '"') {
                    currentLine += '"';
                    i++;
                } else {
                    currentLine += '"';
                    inQuotes = !inQuotes;
                }
            } else if (char === '\n' && !inQuotes) {
                lines.push(currentLine);
                currentLine = '';
            } else if (char === '\r' && !inQuotes) {
                // Skip carriage return
            } else {
                currentLine += char;
            }
        }
        
        if (currentLine) {
            lines.push(currentLine);
        }
        
        const result: any[][] = [];
        for (const line of lines) {
            if (line.trim() === '') {
                result.push([]);
                continue;
            }
            
            const row: any[] = [];
            let currentValue = '';
            let inFieldQuotes = false;
            
            for (let i = 0; i < line.length; i++) {
                const char = line[i];
                
                if (char === '"') {
                    if (inFieldQuotes && line[i + 1] === '"') {
                        currentValue += '"';
                        i++;
                    } else {
                        inFieldQuotes = !inFieldQuotes;
                    }
                } else if (char === ',' && !inFieldQuotes) {
                    row.push(this.parseValue(currentValue));
                    currentValue = '';
                } else {
                    currentValue += char;
                }
            }
            
            row.push(this.parseValue(currentValue));
            result.push(row);
        }
        
        return result;
    }
    
    /**
     * Parse a string value to the appropriate type
     */
    private static parseValue(value: string): any {
        if (value === '' || value === null || value === undefined) {
            return undefined;
        }
        
        // Try to parse as number
        const num = Number(value);
        if (!isNaN(num) && value.trim() !== '') {
            return num;
        }
        
        // Try to parse as boolean
        const lower = value.toLowerCase().trim();
        if (lower === 'true' || lower === 'false') {
            return lower === 'true';
        }
        
        return value;
    }
    
    /**
     * Parse a single CSV file
     */
    public static parseCsvFile(csvPath: string): CsvSheetData {
        const content = fs.readFileSync(csvPath, 'utf8');
        const sheetName = path.basename(csvPath, '.csv');
        return {
            name: sheetName,
            data: this.parseCsv(content)
        };
    }
    
    /**
     * Parse all CSV files in a directory (treats directory like an xlsx file with multiple sheets)
     */
    public static parseCsvDirectory(dirPath: string): CsvSheetData[] {
        const result: CsvSheetData[] = [];
        if (!fs.existsSync(dirPath) || !fs.statSync(dirPath).isDirectory()) {
            return result;
        }
        
        const files = fs.readdirSync(dirPath);
        for (const file of files) {
            if (path.extname(file).toLowerCase() === '.csv') {
                const csvPath = path.join(dirPath, file);
                result.push(this.parseCsvFile(csvPath));
            }
        }
        
        return result;
    }
    
    /**
     * Parse a file path - if it's an xlsx file, check for a corresponding CSV directory
     * Returns data in the same format as node-xlsx.parse()
     */
    public static parse(filePath: string): CsvSheetData[] {
        // First check if it's a directory (CSV sheets directory)
        if (fs.existsSync(filePath) && fs.statSync(filePath).isDirectory()) {
            return this.parseCsvDirectory(filePath);
        }
        
        // Check if it's a CSV file
        if (path.extname(filePath).toLowerCase() === '.csv') {
            return [this.parseCsvFile(filePath)];
        }
        
        // For xlsx files, check if there's a corresponding CSV directory
        const dirPath = filePath.replace(/\.xlsx$/i, '');
        if (fs.existsSync(dirPath) && fs.statSync(dirPath).isDirectory()) {
            return this.parseCsvDirectory(dirPath);
        }
        
        // If no CSV directory found, return empty array
        return [];
    }
    
    /**
     * Check if a path can be parsed as CSV (either CSV file or directory with CSV files)
     */
    public static canParseAsCsv(filePath: string): boolean {
        if (fs.existsSync(filePath) && fs.statSync(filePath).isDirectory()) {
            return true;
        }
        
        if (path.extname(filePath).toLowerCase() === '.csv') {
            return fs.existsSync(filePath);
        }
        
        const dirPath = filePath.replace(/\.xlsx$/i, '');
        return fs.existsSync(dirPath) && fs.statSync(dirPath).isDirectory();
    }
}


