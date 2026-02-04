#pragma once

#include "../include/dns_types.h"
#include <vector>

class StatisticsCalculator {
public:
    static DnsStatistics Calculate(const std::vector<DnsTestResult>& results);
};
