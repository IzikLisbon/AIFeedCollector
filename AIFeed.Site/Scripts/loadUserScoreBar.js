var showScoreBar = function () {
    var margin = { top: 120, right: 20, bottom: 30, left: 40 },
        width = $("#rowWithFillGauges").width() - 100,
        height = $("#rowWithFillGauges").height() * 3;

    var x = d3.scale.ordinal()
        .rangeRoundBands([0, width], .1);

    var y = d3.scale.linear()
        .range([height, 0]);

    var xAxis = d3.svg.axis()
        .scale(x)
        .orient("bottom");

    var yAxis = d3.svg.axis()
        .scale(y)
        .orient("left")
        .ticks(10, "");

    var svg = d3.select("#userScore").append("svg")
        .attr("id", "svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
      .append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    d3.tsv("UserScore", type, function (error, data) {
        x.domain(data.map(function (d) { return d.authorName; }));
        y.domain([0, d3.max(data, function (d) { return d.score; })]);

        svg.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(0," + height + ")")
            .call(xAxis);

        svg.append("g")
            .attr("class", "y axis")
            .call(yAxis)
          .append("text")
            .attr("transform", "rotate(-90)")
            .attr("y", 6)
            .attr("dy", ".71em")
            .style("text-anchor", "end")
            .text("Score");

        svg.selectAll(".bar")
            .data(data)
          .enter().append("rect")
            .attr("class", "bar")
            .attr("x", function (d) { return x(d.authorName); })
            .attr("width", x.rangeBand())
            .attr("y", function (d) { return y(d.score); })
            .attr("height", function (d) { return height - y(d.score); });
    });
};

function type(d) {
    d.score = +d.score;
    return d;
}

showScoreBar();

//$(window).resize(function () {
//    width = $("#rowWithFillGauges").width() - 100;
//    height = $("#rowWithFillGauges").height() * 3;
//    $("#svg").width(width);
//    $("#svg").height(height);
//});