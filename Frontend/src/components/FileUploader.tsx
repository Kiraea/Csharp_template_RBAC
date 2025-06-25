import {type ChangeEvent, useEffect, useState} from "react";

import Papa from 'papaparse';


type UploadStatus = "idle" | "uploading" | "success" | "error"
export default function FileUploader(){
    
    const [file, setFile] = useState<File | null>(null);
    const [columns, setColumns] = useState<string[]>([]);
    const [rows, setRows] = useState<Array<{[key: string] : any}>>([]);
    const [status, setStatus] = useState<UploadStatus>("idle"); 
    
    function handleFileChange(e: ChangeEvent<HTMLInputElement>){
        if (e.target.files){
            setFile(e.target.files[0]);
        }     
    }
    
    useEffect(() => {
        if (file){
            if (file.type === 'text/csv'){
                Papa.parse(file, {header: true, complete : (results :any) => {
                        setColumns(Object.keys(results.data[0]))
                        setRows(results.data);
                    }});    
            }else{
                console.log("NOT CSV");
            }
            
        }
    }, [file]) 

        
    return (
        <div>
            <input type="file" onChange={handleFileChange}/>
            {file && (
                <div>
                    <p>File Name: {file.name}</p>
                    <p>File Type: {file.type}</p>
                    <p>File Size: {(file.size / 1024).toFixed(2)}Kb</p>
                </div>
            )}
            {file && (
                <div> </div>
            )}
            {rows.length !== 0 &&
            <table>
                <thead>
                <tr>
                {columns.map((column,index)=> {
                    return <th key={column}>{column}</th>
                })}
                </tr>
                </thead>
                <tbody>
                {rows.map((row, index) => {
                    return (
                        <tr key={index}>
                            {columns.map((column,index)=> {
                                return (
                                    <td key={index}>{row[column]}</td>
                                )
                            })}
                        </tr>
                    )
                    })}
                </tbody>
            </table>}
            
        </div>
    )
}