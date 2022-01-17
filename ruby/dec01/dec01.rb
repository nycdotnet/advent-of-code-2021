
def count_depth_increase(m)
    result = 0
    for i in 0..m.length() - 2
        if m[i] < m[i+1]
            result += 1
        end
    end
    return result
end



input_file_to_use = "adventofcode2021-dec01/example-input1.txt"
measurements = File.readlines(
    File.join(__dir__, "../../", input_file_to_use),
    encoding: "bom|utf-8")
.map{|l| l.to_i}

answer1 = count_depth_increase(measurements)

puts answer1
