FileRead, Contents, E:\AHK.txt
if not ErrorLevel  ; Successfully loaded.
{
    if(Contents="active")
    {
        Contents := "deactive"
    }else
    {
        Contents := "active"
    }
    FileDelete, E:\AHK.txt
    FileAppend, %Contents%, E:\AHK.txt
    Contents := ""  ; Free the memory.
}else
{
    Contents := "active"
    FileAppend, %Contents%, E:\AHK.txt
}