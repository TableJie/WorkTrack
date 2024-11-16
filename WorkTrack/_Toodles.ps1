$record_file = "_target.txt"
$output_file = "_export.txt"
# 取得當前腳本所在資料夾的路徑
$script_dir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Clear the output file
Clear-Content "$script_dir\$output_file"
# Read the record file line by line
Get-Content "$script_dir\$record_file" | ForEach-Object {
    $relative_path = $_  # 修正：將 $ 改為 $_
    # 如果讀到 "END" 則跳出迴圈
    if ($relative_path -eq "END") {
        break
    }
    # 拼接完整路徑
    $file_name = Join-Path $script_dir $relative_path
    $file_base_name = Split-Path $file_name -Leaf  # 只取得檔案名稱，不包含路徑
    # Check if the file exists
    if (Test-Path $file_name) {
        # Read the file content and append to the output file
        $content = Get-Content $file_name -Raw
        Add-Content "$script_dir\$output_file" "$content`nThis file is: $relative_path"
        Add-Content "$script_dir\$output_file" ""
    } else {
        Add-Content "$script_dir\$output_file" "File $relative_path does not exist"
        Add-Content "$script_dir\$output_file" ""
    }
}
Write-Output "Output completed: $output_file"  # 移出迴圈外

# 加入一串文字
Add-Content "$script_dir\$output_file" "Based on the above files, solve my requirements. Please answer in Traditional Chinese, prioritizing solutions that reduce system load, with maintainability as secondary. Responses should be clear, concise, and to the point. Since there are many different files, please first provide the file names and present the solutions in a before-and-after comparison format. For both the before and after content, only show the modified sections, no need to display everything. My requirement is:"
# Open the output file
Start-Process notepad.exe "$script_dir\$output_file"