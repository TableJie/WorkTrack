# Read the record file name from the txt file
$record_file = "_target.txt"
$output_file = "_export.txt"

# Clear the output file
Clear-Content $output_file

# Read the record file line by line
Get-Content $record_file | ForEach-Object {
    $file_name = $_
    # Check if the file exists
    if (Test-Path $file_name) {
        # Read the file content and append to the output file
        $content = Get-Content $file_name -Raw
        Add-Content $output_file "$content`nThis is $file_name"
        Add-Content $output_file ""
    } else {
        Add-Content $output_file "File $file_name does not exist"
        Add-Content $output_file ""
    }
	
	Write-Output "Output completed: $output_file"
}


	Add-Content $output_file "Based on the above files, solve my requirements. Please answer in Traditional Chinese, prioritizing solutions that reduce system load, with maintainability as secondary. Responses should be clear, concise, and to the point. Since there are many different files, please first provide the file names and present the solutions in a before-and-after comparison format. For both the before and after content, only show the modified sections, no need to display everything. My requirement is:"	

# Open the output file
Start-Process notepad.exe $output_file
