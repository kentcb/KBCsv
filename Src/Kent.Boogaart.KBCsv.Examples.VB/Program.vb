Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports Kent.Boogaart.KBCsv.Extensions
Imports System.Windows.Forms
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Threading

Module Program
    Sub Main()
        Dim examples As Dictionary(Of Char, MethodInfo) = GetType(Program).GetMethods(BindingFlags.Static Or BindingFlags.NonPublic).Where(Function(x As MethodInfo) x.Name <> "Main" AndAlso x.GetCustomAttribute(Of CompilerGeneratedAttribute)() Is Nothing).Select(Function(m As MethodInfo, i As Integer) New With {.Method = m, .Key = Chr(Asc("a") + i)}).ToDictionary(Function(e) e.Key, Function(e) e.Method)

        While True
            Console.Clear()
            Console.WriteLine("Choose an example to run:")

            For Each example As KeyValuePair(Of Char, MethodInfo) In examples
                Console.WriteLine("  {0}. {1}", example.Key, example.Value.Name)
            Next

            Dim choice As ConsoleKeyInfo = Console.ReadKey()

            If examples.ContainsKey(choice.KeyChar) Then
                Console.CursorLeft = 0
                Console.WriteLine(" ")

                Dim result As Task = TryCast(examples(choice.KeyChar).Invoke(Nothing, Nothing), Task)

                If result IsNot Nothing Then
                    result.Wait()
                End If

                Console.WriteLine()
                Console.WriteLine("Done - press a key to choose another example")
                Console.ReadKey()
            End If
        End While
    End Sub

#Region "ReadCSVFromString"

    Private Sub ReadCSVFromString()
        Dim csv As String = "Kent,33" & vbCrLf &
            "Belinda,34" & vbCrLf &
            "Tempany,8"

        Using reader = CsvReader.FromCsvString(csv)
            While reader.HasMoreRecords
                Dim dataRecord As DataRecord = reader.ReadDataRecord()
                Console.WriteLine("{0} is {1} years old.", dataRecord.Item(0), dataRecord.Item(1))
            End While
        End Using
    End Sub

#End Region

#Region "ReadCSVFromStringPreservingWhiteSpace"

    Private Sub ReadCSVFromStringPreservingWhiteSpace()
        Dim csv As String = "Kent   ,33" & vbCrLf &
            "Belinda,34" & vbCrLf &
            "Tempany, 8"

        Using reader = CsvReader.FromCsvString(csv)
            reader.PreserveLeadingWhiteSpace = True
            reader.PreserveTrailingWhiteSpace = True

            While reader.HasMoreRecords
                Dim dataRecord As DataRecord = reader.ReadDataRecord()
                Console.WriteLine("{0} is {1} years old.", dataRecord.Item(0), dataRecord.Item(1))
            End While
        End Using
    End Sub

#End Region

#Region "ReadTabDelimitedDataFromFile"

    Private Sub ReadTabDelimitedDataFromFile()
        Using reader = New CsvReader("PlanetaryData.tdv")
            reader.ValueSeparator = Constants.vbTab
            reader.ValueDelimiter = "'"

            While reader.HasMoreRecords
                Dim dataRecord As DataRecord = reader.ReadDataRecord()
                Console.WriteLine("{0} is nicknamed {1}.", dataRecord.Item(0), dataRecord.Item(dataRecord.Count - 1))
            End While
        End Using
    End Sub

#End Region

#Region "ReadCSVFromFile"

    Private Sub ReadCSVFromFile()
        Using reader = New CsvReader("PlanetaryData.csv")
            ' the CSV file has a header record, so we read that first
            reader.ReadHeaderRecord()

            While reader.HasMoreRecords
                Dim dataRecord As DataRecord = reader.ReadDataRecord()

                ' since the reader has a header record, we can access data by column names as well as by index
                Console.WriteLine("{0} is nicknamed {1}.", dataRecord.Item("Name"), dataRecord.Item("Nickname"))
            End While
        End Using
    End Sub

#End Region

#Region "ReadCSVFromStream"

    Private Sub ReadCSVFromStream()
        Using stream = New FileStream("PlanetaryData.csv", FileMode.Open)
            Using reader = New CsvReader(stream, Encoding.UTF8)
                reader.ReadHeaderRecord()

                While reader.HasMoreRecords
                    Dim dataRecord As DataRecord = reader.ReadDataRecord()
                    Console.WriteLine("{0} is nicknamed {1}.", dataRecord.Item("Name"), dataRecord.Item("Nickname"))
                End While
            End Using
        End Using
    End Sub

#End Region

#Region "ReadCSVFromFileWithExplicitHeader"

    Private Sub ReadCSVFromFileWithExplicitHeader()
        Using reader = New CsvReader("PlanetaryData_NoHeader.csv")
            reader.HeaderRecord = New HeaderRecord("OfficialName", "NickName")

            While reader.HasMoreRecords
                Dim dataRecord As DataRecord = reader.ReadDataRecord()
                Console.WriteLine("{0} is nicknamed {1}.", dataRecord.Item("OfficialName"), dataRecord.Item("NickName"))
                reader.SkipRecord()
            End While
        End Using
    End Sub

#End Region

#Region "ReadCSVFromFileAsynchronously"

    Private Async Function ReadCSVFromFileAsynchronously() As Task
        Using textReader = New StreamReader("PlanetaryData.csv")
            Using reader = New CsvReader(textReader, True)
                Await reader.ReadHeaderRecordAsync()

                ' realistically, you'll probably want a larger buffer, but this suffices for demonstration purposes
                Dim buffer(4) As DataRecord

                While reader.HasMoreRecords
                    Dim read As Integer = Await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length)

                    For i As Integer = 0 To read - 1
                        Console.WriteLine("{0} is nicknamed {1}.", buffer(i).Item("Name"), buffer(i).Item("Nickname"))
                    Next
                End While
            End Using
        End Using
    End Function

#End Region

#Region "WriteCSVToString"

    Private Sub WriteCSVToString()
        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                writer.WriteRecord("Name", "Age")
                writer.WriteRecord("Kent", "33")
                writer.WriteRecord("Belinda", "34")
                writer.WriteRecord("Tempany", "8")
            End Using

            Console.WriteLine(stringWriter.ToString())
        End Using
    End Sub

#End Region

#Region "WriteCSVToFile"

    Private Sub WriteCSVToFile()
        Using writer = New CsvWriter("Output.csv")
            writer.ForceDelimit = True

            writer.WriteRecord("Name", "Age")
            writer.WriteRecord("Kent", "33")
            writer.WriteRecord("Belinda", "34")
            writer.WriteRecord("Tempany", "8")

            Console.WriteLine("{0} records written", writer.RecordNumber)
        End Using
    End Sub

#End Region

#Region "WriteCSVToStreamWithForcedDelimiting"

    Private Sub WriteCSVToStreamWithForcedDelimiting()
        Using memoryStream = New MemoryStream()
            Using writer = New CsvWriter(memoryStream, Encoding.ASCII, True)
                writer.ForceDelimit = True

                writer.WriteRecord("Name", "Age")
                writer.WriteRecord("Kent", "33")
                writer.WriteRecord("Belinda", "34")
                writer.WriteRecord("Tempany", "8")
            End Using

            Console.WriteLine(Encoding.ASCII.GetString(memoryStream.ToArray()))
        End Using
    End Sub

#End Region

#Region "ReadCSVFromFileAndWriteToTabDelimitedFile"

    Private Async Function ReadCSVFromFileAndWriteToTabDelimitedFile() As Task
        Using reader = New CsvReader("PlanetaryData.csv")
            Using writer = New CsvWriter("PlanetaryData_Modified.csv")
                writer.ValueSeparator = Constants.vbTab
                writer.ValueDelimiter = "'"

                ' realistically, you'll probably want a larger buffer, but this suffices for demonstration purposes
                Dim buffer(4) As DataRecord

                While reader.HasMoreRecords
                    Dim read As Integer = Await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length)
                    Await writer.WriteRecordsAsync(buffer, 0, read)
                End While
            End Using
        End Using
    End Function

#End Region

#Region "FillDataTableFromCSVFile"

    Private Sub FillDataTableFromCSVFile()
        Dim table As New DataTable()

        Using reader = New CsvReader("PlanetaryData.csv")
            reader.ReadHeaderRecord()
            table.Fill(reader)
        End Using

        Console.WriteLine("Table contains {0} rows.", table.Rows.Count)
    End Sub

#End Region

#Region "WriteDataTableToCSV"

    Private Sub WriteDataTableToCSV()
        Dim table As New DataTable()
        table.Columns.Add("Name")
        table.Columns.Add("Age")
        table.Rows.Add("Kent", 33)
        table.Rows.Add("Belinda", 34)
        table.Rows.Add("Tempany", 8)
        table.Rows.Add("Xak", 0)

        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                table.WriteCsv(writer)
            End Using

            Console.WriteLine("CSV: {0}", stringWriter)
        End Using
    End Sub

#End Region

#Region "FillDataTableFromCSVFileThenWriteSomeToStringAsynchronously"

    Private Async Function FillDataTableFromCSVFileThenWriteSomeToStringAsynchronously() As Task
        Dim table As New DataTable()

        Using reader = New CsvReader("PlanetaryData.csv")
            Await reader.ReadHeaderRecordAsync()
            Await table.FillAsync(reader)
        End Using

        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                table.WriteCsv(writer, False, 5)
            End Using

            Console.WriteLine("CSV: {0}", stringWriter)
        End Using
    End Function

#End Region

#Region "WriteScreenInformationToCSV"

    Private Sub WriteScreenInformationToCSV()
        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                Screen.AllScreens.WriteCsv(writer)
                writer.Flush()

                Console.WriteLine(stringWriter)
            End Using
        End Using
    End Sub

#End Region

#Region "WriteSelectedProcessInformationCSVAsynchronously"

    Private Async Function WriteSelectedProcessInformationCSVAsynchronously() As Task
        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                Await Process.GetProcesses().WriteCsvAsync(writer, True, {"Id", "ProcessName", "WorkingSet64"})
                Await writer.FlushAsync()

                Console.WriteLine(stringWriter)
            End Using
        End Using
    End Function

#End Region

#Region "CopyCSVFileToStringWriter"

    Private Sub CopyCSVFileToStringWriter()
        Using stringWriter = New StringWriter()
            Using reader = New CsvReader("PlanetaryData.csv")
                Using writer = New CsvWriter(stringWriter)
                    writer.ValueSeparator = Constants.vbTab
                    writer.ValueDelimiter = "'"

                    reader.CopyTo(writer)
                End Using
            End Using

            Console.WriteLine(stringWriter)
        End Using
    End Sub

#End Region

End Module
