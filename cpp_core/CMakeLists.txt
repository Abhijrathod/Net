cmake_minimum_required(VERSION 3.20)
project(dns_core VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Output directory
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# Include directories
include_directories(${CMAKE_SOURCE_DIR}/include)

# Source files
set(SOURCES
    src/api.cpp
    src/core.cpp
    src/network_windows.cpp
    src/dns_catalog.cpp
    src/dns_validator.cpp
    src/dns_latency.cpp
    src/dns_packet.cpp
    src/dns_statistics.cpp
    src/dns_apply_windows.cpp
    src/dns_resolver.cpp
    src/json_serializer.cpp
    src/logger.cpp
    src/utils.cpp
)

# Create DLL
add_library(dns_core SHARED ${SOURCES})

# Link libraries
target_link_libraries(dns_core
    iphlpapi
    ws2_32
    netapi32
)

# Export symbols
set_target_properties(dns_core PROPERTIES
    CXX_VISIBILITY_PRESET default
    VISIBILITY_INLINES_HIDDEN OFF
)

# Windows-specific settings
if(WIN32)
    target_compile_definitions(dns_core PRIVATE
        _WINDLL
        _USRDLL
        DNS_CORE_EXPORTS
    )
endif()

# Copy DLL to output directory for easy access
add_custom_command(TARGET dns_core POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    $<TARGET_FILE:dns_core>
    ${CMAKE_BINARY_DIR}/bin/dns_core.dll
)
