Sub AutomateBuildingVariants()
    Application.Calculation = xlCalculationAutomatic

    Dim wb As Workbook
    Dim inputSheetProject As Worksheet, inputSheetEmbodied As Worksheet
    Dim outputSheet As Worksheet
    Dim sheetName As String
    Dim sheetIndex As Integer
    Dim sector As Variant, subSector As Variant, gia As Double
    Dim perimeter As Double, footprint As Double, width As Double, height As Double
    Dim storeysAbove As Integer, storeysBelow As Integer, glazingRatio As Double
    Dim rowCounter As Long, sheetCounter As Integer
    Dim buildingElements As Variant, materialOptions As Object
    Dim currentMaterials As Variant

    Dim iterationCount As Long
    Dim userLimit As Long
    userLimit = InputBox("Enter how many data points you want.", "Set Limit", 1000) ' Default 1000 values

    ' Setup workbook And sheets
    Set wb = ThisWorkbook
    Set inputSheetProject = wb.Sheets("0. INPUT Project Details")
    Set inputSheetEmbodied = wb.Sheets("2. INPUT Embodied Carbon")

    ' Create unique sheet name
    sheetIndex = 1
    sheetName = "Results " & sheetIndex
    While SheetExists(sheetName, wb)
        sheetIndex = sheetIndex + 1
        sheetName = "Results " & sheetIndex
    Wend

    ' Add New sheet With unique name
    Set outputSheet = wb.Sheets.Add(After:=wb.Sheets(wb.Sheets.Count))
    outputSheet.Name = sheetName

    ' Initialize building elements And their corresponding material options
    Set materialOptions = CreateObject("Scripting.Dictionary")
    buildingElements = Array("Piles", "Pile caps", "Capping beams", "Raft", "Basement walls", "Lowest floor slab", _
    "Ground insulation", "Core structure", "Columns", "Beams", "Secondary beams", "Floor slab", _
    "Joisted floors", "Roof", "Roof insulation", "Roof finishes", "Facade", "Wall insulation", _
    "Glazing", "Window frames", "Partitions", "Ceilings", "Floors", "Services")

    ' Initialize building elements And their corresponding material options
    Set materialOptions = CreateObject("Scripting.Dictionary")

    ' Add material options For each building element
    materialOptions.Add "Piles", Array("RC 32/40 (50kg/m3 reinforcement)", "Steel", "")
    materialOptions.Add "Pile caps", Array("RC 32/40 (200kg/m3 reinforcement)", "")
    materialOptions.Add "Capping beams", Array("RC 32/40 (200kg/m3 reinforcement)", "Foamglass (domestic only)", "")
    materialOptions.Add "Raft", Array("RC 32/40 (150kg/m3 reinforcement)", "")
    materialOptions.Add "Basement walls", Array("RC 32/40 (125kg/m3 reinforcement)", "")
    materialOptions.Add "Lowest floor slab", Array("RC 32/40 (150kg/m3 reinforcement)", "Beam And Block", "")
    materialOptions.Add "Ground insulation", Array("EPS", "XPS", "Glass mineral wool", "")
    materialOptions.Add "Core structure", Array("CLT", "Precast RC 32/40 (100kg/m3 reinforcement)", "RC 32/40 (100kg/m3 reinforcement)", "")
    materialOptions.Add "Columns", Array("Glulam", "Iron (existing buildings)", "Precast RC 32/40 (300kg/m3 reinforcement)", "RC 32/40 (300kg/m3 reinforcement)", "Steel", "")
    materialOptions.Add "Beams", Array("Glulam", "Iron (existing buildings)", "Precast RC 32/40 (250kg/m3 reinforcement)", "RC 32/40 (250kg/m3 reinforcement)", "Steel", "")
    materialOptions.Add "Secondary beams", Array("Glulam", "Iron (existing buildings)", "Precast RC 32/40 (250kg/m3 reinforcement)", "RC 32/40 (250kg/m3 reinforcement)", "Steel", "")
    materialOptions.Add "Floor slab", Array("CLT", "Precast RC 32/40 (100kg/m3 reinforcement)", "RC 32/40 (100kg/m3 reinforcement)", "Steel Concrete Composite", "")
    materialOptions.Add "Joisted floors", Array("JJI Engineered Joists + OSB topper", "Timber Joists + OSB topper (Domestic)", "Timber Joists + OSB topper (Office)", "")
    materialOptions.Add "Roof", Array("CLT", "Metal Deck", "Precast RC 32/40 (100kg/m3 reinforcement)", "RC 32/40 (100kg/m3 reinforcement)", "Steel Concrete Composite", "Timber Cassette", "Timber Pitch Roof", "")
    materialOptions.Add "Roof insulation", Array("Cellulose, loose fill", "EPS", "Expanded Perlite", "Expanded Vermiculite", "Glass mineral wool", "PIR", "Rockwool", "Sheeps wool", "Vacuum Insulation", "Woodfibre", "XPS", "")
    materialOptions.Add "Roof finishes", Array("Aluminium", "Asphalt (Mastic)", "Asphalt (Polymer modified)", "Bitumous Sheet", "Ceramic tile", "Fibre cement tile", "Green Roof", "Roofing membrane (PVC)", "Slate tile", "Zinc Standing Seam", "")
    materialOptions.Add "Facade", Array("Blockwork With Brick", "Blockwork With render", "Blockwork With Timber", "Curtain Walling", "Load Bearing Precast Concrete Panel", "Load Bearing Precast Concrete With Brick Slips", "Party Wall Blockwork", "Party Wall Brick", "Party Wall Timber Cassette", "SFS With Aluminium Cladding", "SFS With Brick", "SFS With Ceramic Tiles", "SFS With Granite", "SFS With Limestone", "SFS With Zinc Cladding", "Solid Brick, single leaf", "Timber Cassette Panel With brick", "Timber Cassette Panel With Cement Render", "Timber Cassette Panel With Larch Cladding", "Timber Cassette Panel With Lime Render", "Timber SIPs With Brick", "")
    materialOptions.Add "Wall insulation", Array("Cellulose, loose fill", "EPS", "Expanded Perlite", "Expanded Vermiculite", "Glass mineral wool", "PIR", "Rockwool", "Sheeps wool", "Vacuum Insulation", "Woodfibre", "XPS", "")
    materialOptions.Add "Glazing", Array("Triple Glazing", "Double Glazing", "Single Glazing", "")
    materialOptions.Add "Window frames", Array("Al/Timber Composite", "Aluminium", "Steel (single glazed)", "Solid softwood timber frame", "uPVC", "")
    materialOptions.Add "Partitions", Array("CLT", "Plasterboard + Steel Studs", "Plasterboard + Timber Studs", "Plywood + Timber Studs", "Blockwork", "")
    materialOptions.Add "Ceilings", Array("Exposed Soffit", "Plasterboard", "Steel grid system", "Steel tile", "Steel tile With 18mm acoustic pad", "Suspended plasterboard", "")
    materialOptions.Add "Floors", Array("70mm screed", "Carpet", "Earthenware tile", "Raised floor", "Solid timber floorboards", "Stoneware tile", "Terrazzo", "Vinyl", "")
    materialOptions.Add "Services", Array("Low", "Medium", "High", "")

    ' Initialize sector options And Sub-sectors
    Dim sectorOptions As Variant
    Dim allSubSectors As Object
    Set allSubSectors = CreateObject("Scripting.Dictionary")
    sectorOptions = Array("Housing", "Office")
    allSubSectors.Add "Housing", Array("Flat/maisonette", "Single family house", "Multi-family (< 6 storeys)", "Multi-family (6 - 15 storeys)", "Multi-family (> 15 storeys)")
    allSubSectors.Add "Office", Array("Office")

    ' Prepare header For the first Results Sheet
    Call PrepareResultsSheetHeader(outputSheet, buildingElements)
    ' Counter initialization
    rowCounter = 2
    sheetCounter = 1
    startRow = 29

    ' Random selection process
    Do While iterationCount < userLimit
        ' Constraint dims
        Dim hasPiles As Boolean
        Dim hasCappingbeams As Boolean
        Dim hasPilecaps As Boolean
        Dim hasFloorSlab As Boolean

        hasPiles = False
        hasCappingbeams = False
        hasPilecaps = False
        hasFloorSlab = False

        For Each sector In sectorOptions
            For Each subSector In allSubSectors(sector)
                gia = Int((20000 + 1) * Rnd)
                perimeter = Int((5000 - 100 + 1) * Rnd + 100)
                footprint = Int((10000 - 100 + 1) * Rnd + 100)
                width = Int((200 - 10 + 1) * Rnd + 10)
                height = Int((6 - 2.3 + 1) * Rnd * 10) / 10 + 2.3
                storeysAbove = Int((60 - 1 + 1) * Rnd + 1)
                storeysBelow = Int((5 - 0 + 1) * Rnd)
                glazingRatio = Int((80 - 10 + 1) * Rnd + 10)

                ' Initialize current materials array
                ReDim currentMaterials(UBound(buildingElements))

                ' Recursive Call To process all material combinations
                ProcessMaterials 0, buildingElements, materialOptions, currentMaterials, _
                outputSheet, rowCounter, sector, subSector, gia, _
                perimeter, footprint, width, height, storeysAbove, _
                storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, hasPilecaps, _
                hasFloorSlab

                iterationCount = iterationCount + 1
                If iterationCount >= userLimit Then
                 Exit Do
                End If

                If rowCounter Mod 60 = 1 Then
                    Application.Goto outputSheet.Cells(rowCounter - 1, 1), True
                End If

            Next subSector
            If iterationCount >= userLimit Then
             Exit Do
            End If
        Next sector
    Loop
    MsgBox "Automation complete!"
End Sub
' Recursive Function To handle all material combinations
Sub ProcessMaterials(Byval elementIndex As Integer, Byref buildingElements As Variant, Byref materialOptions As Object, _
    Byref currentMaterials As Variant, Byref outputSheet As Worksheet, Byref rowCounter As Long, _
    Byval sector As Variant, Byval subSector As Variant, Byval gia As Double, _
    Byval perimeter As Double, Byval footprint As Double, Byval width As Double, _
    Byval height As Double, Byval storeysAbove As Integer, Byval storeysBelow As Integer, _
    Byval glazingRatio As Double, Byval startRow As Integer, Byval hasPiles As Boolean, _
    Byval hasCappingbeams As Boolean, Byval hasPilecaps As Boolean, Byval hasFloorSlab As Boolean)

    If elementIndex > UBound(buildingElements) Then
        ' All elements have materials assigned, output the results
        RecordResults currentMaterials, outputSheet, rowCounter, sector, subSector, gia, _
        perimeter, footprint, width, height, storeysAbove, storeysBelow, glazingRatio
        rowCounter = rowCounter + 1
     Exit Sub
    End If

    Dim element As String
    element = buildingElements(elementIndex)
    Dim materials As Variant
    materials = materialOptions(element)

    Randomize ' Initialize random number generator
    Dim i As Integer
    i = Int((UBound(materials) - LBound(materials) + 1) * Rnd + LBound(materials))

    ' Skip logic - If realistic building conditions aren't met
    Select Case element
     Case "Raft"
        If hasCappingbeams Or hasPilecaps Then
            ProcessMaterials elementIndex + 1, buildingElements, materialOptions, currentMaterials, _
            outputSheet, rowCounter, sector, subSector, gia, perimeter, footprint, width, _
            height, storeysAbove, storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, _
            hasPilecaps, hasFloorSlab
         Exit Sub
        End If

     Case "Pile caps", "Capping beams"
        If Not hasPiles Then
            ProcessMaterials elementIndex + 1, buildingElements, materialOptions, currentMaterials, _
            outputSheet, rowCounter, sector, subSector, gia, perimeter, footprint, width, _
            height, storeysAbove, storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, _
            hasPilecaps, hasFloorSlab
         Exit Sub
        End If

     Case "Basement walls"
        If storeysBelow = 0 Then
            ProcessMaterials elementIndex + 1, buildingElements, materialOptions, currentMaterials, _
            outputSheet, rowCounter, sector, subSector, gia, perimeter, footprint, width, _
            height, storeysAbove, storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, _
            hasPilecaps, hasFloorSlab
         Exit Sub
        End If

     Case "Joisted floors"
        If hasFloorSlab Then
            ProcessMaterials elementIndex + 1, buildingElements, materialOptions, currentMaterials, _
            outputSheet, rowCounter, sector, subSector, gia, perimeter, footprint, width, _
            height, storeysAbove, storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, _
            hasPilecaps, hasFloorSlab
         Exit Sub
        End If
    End Select

    currentMaterials(elementIndex) = materials(i) ' Assign And log the selected material

    If materials(i) <> "" Then
        Select Case element
         Case "Piles"
            hasPiles = True

         Case "Pile caps"
            hasPilecaps = True

         Case "Capping beams"
            hasCappingbeams = True

         Case "Floor slab"
            hasFloorSlab = True
        End Select
    End If


    ' Set the material For the current building element in the input sheet
    ThisWorkbook.Sheets("2. INPUT Embodied Carbon").Cells(startRow + elementIndex, 3).Value = materials(i)

    ' Recursively process the Next element With the Next index
    ProcessMaterials elementIndex + 1, buildingElements, materialOptions, currentMaterials, _
    outputSheet, rowCounter, sector, subSector, gia, perimeter, footprint, width, _
    height, storeysAbove, storeysBelow, glazingRatio, startRow, hasPiles, hasCappingbeams, _
    hasPilecaps, hasFloorSlab
End Sub
' Function To record results
Sub RecordResults(Byref currentMaterials As Variant, Byref outputSheet As Worksheet, Byval rowCounter As Long, _
    Byval sector As Variant, Byval subSector As Variant, Byval gia As Double, _
    Byval perimeter As Double, Byval footprint As Double, Byval width As Double, _
    Byval height As Double, Byval storeysAbove As Integer, Byval storeysBelow As Integer, _
    Byval glazingRatio As Double)

    Dim wb As Workbook
    Set wb = ThisWorkbook
    embodiedCarbon = wb.Sheets("5. OUTPUT Machine").Cells(19, 2).Value

    outputSheet.Cells(rowCounter, 1).Value = sector
    outputSheet.Cells(rowCounter, 2).Value = subSector
    outputSheet.Cells(rowCounter, 3).Value = gia
    outputSheet.Cells(rowCounter, 4).Value = perimeter
    outputSheet.Cells(rowCounter, 5).Value = footprint
    outputSheet.Cells(rowCounter, 6).Value = width
    outputSheet.Cells(rowCounter, 7).Value = height
    outputSheet.Cells(rowCounter, 8).Value = storeysAbove
    outputSheet.Cells(rowCounter, 9).Value = storeysBelow
    outputSheet.Cells(rowCounter, 10).Value = glazingRatio

    ' Output materials
    Dim colIdx As Integer
    colIdx = 11 
    Dim idx As Integer
    For idx = LBound(currentMaterials) To UBound(currentMaterials)
        outputSheet.Cells(rowCounter, colIdx).Value = currentMaterials(idx)
        colIdx = colIdx + 1
    Next idx

    outputSheet.Cells(rowCounter, colIdx).Value = embodiedCarbon
End Sub

Function SheetExists(sheetName As String, wb As Workbook) As Boolean
    Dim tmpSheet As Worksheet
    On Error Resume Next
    Set tmpSheet = wb.Sheets(sheetName)
    On Error Goto 0
        SheetExists = Not tmpSheet Is Nothing
End Function

Private Sub PrepareResultsSheetHeader(sheet As Worksheet, buildingElements As Variant)
    Dim col As Integer
    sheet.Cells(1, 1).Value = "Sector"
    sheet.Cells(1, 2).Value = "Sub-Sector"
    sheet.Cells(1, 3).Value = "GIA (m2)"
    sheet.Cells(1, 4).Value = "Building Perimeter"
    sheet.Cells(1, 5).Value = "Building Footprint"
    sheet.Cells(1, 6).Value = "Building Width"
    sheet.Cells(1, 7).Value = "Floor-To-Floor Height"
    sheet.Cells(1, 8).Value = "No. Storeys Ground & Above"
    sheet.Cells(1, 9).Value = "No. Storeys Below Ground"
    sheet.Cells(1, 10).Value = "Glazing Ratio"

    col = 11
    Dim i As Integer
    For Each element In buildingElements
        Debug.Print element
        sheet.Cells(1, col).Value = element & " Material"
        col = col + 1
    Next element

    sheet.Cells(1, col).Value = "Embodied Carbon (kgCO2e/m2)"
End Sub