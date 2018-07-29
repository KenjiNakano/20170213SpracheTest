def make_createtrg_statement(partial_lines)
    lines = []
    partial_lines.each do |l|
        newl = l.gsub("\t", " ").gsub("\n", " ").gsub("[", "").gsub("]", "")
        lines.push(newl)
    end
    lines.join("")
end

def parse_creattrg_statement(createtrg_statement)
    p createtrg_statement

    trg_scm = ""
    trg_name = ""
    tbl_scm = ""
    tbl_name = ""

    state = "read_create"

    create = ""
    trigger = ""
    on = ""

    createtrg_statement.chars do |c|
        if (state == "read_create") then
            if (create == "" and c == " ") then
                #連続した空白は読み飛ばす
                next
            end
            if (create != "" and c == " ") then
                if (create != "CREATE") then
                    return false, "create != ""CREATE""", create
                end
                state = "read_trigger"
                next
            end
            create += c
        elsif (state == "read_trigger") then
            if (trigger == "" and c == " ") then
                #連続した空白は読み飛ばす
                next
            end
            if (trigger != "" and c == " ") then
                if (trigger != "TRIGGER") then
                    return false, "trigger != ""TRIGGER""", trigger
                end
                state = "read_trg_scm"
                next 
            end
            trigger +=c
        elsif (state == "read_trg_scm") then
            if (c == ".") then
                state = "read_trg_name"
                next
            end
            trg_scm += c
        elsif (state == "read_trg_name") then
            if (trg_name == "" and c == " ") then
                #連続した空白は読み飛ばす
                next
            end
            if (trg_name != "" and c == " ") then
                state = "read_on"
                next
            end
            trg_name += c
        elsif (state == "read_on") then
            if (on == "" and c == " ") then
                #連続した空白は読み飛ばす
                next
            end
            if (c == " ") then
                if (on != "ON") then
                    return false, "on != ""ON"""
                end
                state = "read_tbl_scm"
                next
            end
            on +=c 
        elsif (state == "read_tbl_scm") then
            if (c == ".") then
                state = "read_tbl_name"
                next
            end
            tbl_scm +=c 
        elsif (state == "read_tbl_name") then
            if (c == " ") then
                state = "end"
                next
            end
            tbl_name += c
        end
    end

    if (state != "end") then
        return false, "state != ""end""", state
    end

    return true, trg_scm.gsub(" ", ""), trg_name.gsub(" ", ""), tbl_scm.gsub(" ", ""), tbl_name.gsub(" ", "")
end

def conv_trg_sql(file_name)
    file_name = file_name.sub(".sql", "")
    lines = []
    File.open(file_name + ".sql") do |file|
        file.each_line do |labmen|
            lines.push(labmen)
        end
    end

    File.rename(file_name + ".sql", "bk/" + file_name + ".sql")

    result = false
    trg_scm = ""
    trg_name = ""
    tbl_scm = ""
    tbl_name = ""

    i = 0
    lines.each do |l|
        if (l.start_with?("CREATE TRIGGER")) then
            createtrg_statement = make_createtrg_statement(lines[i, 2])
            result, trg_scm, trg_name, tbl_scm, tbl_name = parse_creattrg_statement(createtrg_statement)
            if (result == false) then
                p result, trg_scm, trg_name, tbl_scm, tbl_name
                File.rename("bk/" + file_name + ".sql", file_name + ".sql")
                return
            end
        end
        i = i + 1
    end

    finallines = []

    finallines.push("DROP TRIGGER [" + trg_scm + "].[" + trg_name + "]")
    finallines.push("GO")
    finallines.push("")
    finallines.push("SET ANSI_NULLS ON")
    finallines.push("GO")
    finallines.push("")
    finallines.push("SET QUOTED_IDENTIFIER OFF")
    finallines.push("GO")
    finallines.push("")

    lines.each do |l|
        finallines.push(l)
    end

    finallines.push("")
    finallines.push("GO")
    finallines.push("")
    finallines.push("ALTER TABLE [" + tbl_scm + "].[" + tbl_name + "] ENABLE TRIGGER [" + trg_name + "]")
    finallines.push("GO")

    File.open(file_name + ".sql", "w") do |file|
        finallines.each do |fl|
            file.puts(fl)
        end
    end
end

Dir.mkdir("bk")
Dir.glob("*.Trigger.sql").each do |filename|
    conv_trg_sql(filename)
end
