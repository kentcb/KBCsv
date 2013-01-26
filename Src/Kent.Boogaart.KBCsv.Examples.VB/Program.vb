Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports Kent.Boogaart.KBCsv.Extensions
Imports System.Windows.Forms

Module Program

    Sub Main()
        ' uncomment the example you would like to run
        'Example1()
        'Example2()
        'Example3()
        'Example4()
        'Example5().Wait()
        'Example6()
        'Example7()
        'Example8().Wait()
        'Example9()
        'Example10()
        'Example11().Wait()
        'Example12()
        'Example13().Wait()
        'Example14()

        Console.WriteLine()
        Console.WriteLine("DONE - any key to exit")
        Console.ReadKey()
    End Sub

#Region "Example 1"

    Sub Example1()
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

#Region "Example 2"

    Sub Example2()
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

#Region "Example 3"

    Sub Example3()
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

#Region "Example 4"

    Sub Example4()
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

#Region "Example 5"

    Async Function Example5() As Task
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

#Region "Example 6"

    Sub Example6()
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

#Region "Example 7"

    Sub Example7()
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

#Region "Example 8"

    Async Function Example8() As Task
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

#Region "Example 9"

    Sub Example9()
        Dim table As New DataTable()

        Using reader = New CsvReader("PlanetaryData.csv")
            reader.ReadHeaderRecord()
            table.Fill(reader)
        End Using

        Console.WriteLine("Table contains {0} rows.", table.Rows.Count)
    End Sub

#End Region

#Region "Example 10"

    Sub Example10()
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

#Region "Example 11"

    Async Function Example11() As Task
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

#Region "Example 12"

    Sub Example12()
        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                Screen.AllScreens.WriteCsv(writer)
                writer.Flush()

                Console.WriteLine(stringWriter)
            End Using
        End Using
    End Sub

#End Region

#Region "Example 13"

    Async Function Example13() As Task
        Using stringWriter = New StringWriter()
            Using writer = New CsvWriter(stringWriter)
                Await Process.GetProcesses().WriteCsvAsync(writer, True, {"Id", "ProcessName", "WorkingSet64"})
                Await writer.FlushAsync()

                Console.WriteLine(stringWriter)
            End Using
        End Using
    End Function

#End Region

#Region "Example 14"

    Sub Example14()
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
