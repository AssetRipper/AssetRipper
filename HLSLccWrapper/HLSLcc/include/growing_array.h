#pragma once

namespace HLSLcc
{
    // A vector that automatically grows when written to, fills the intermediate ones with default value.
    // Reading from an index returns the default value if attempting to access out of bounds.
    template<class T> class growing_vector
    {
    public:
        growing_vector() : data() {}

        std::vector<T> data;

        T & operator[](std::size_t idx)
        {
            if (idx >= data.size())
                data.resize((idx + 1) * 2);
            return data[idx];
        }

        const T & operator[](std::size_t idx) const
        {
            static T defaultValue = T();
            if (idx >= data.size())
                return defaultValue;
            return data[idx];
        }
    };

    // Same but with bool specialization
    template<> class growing_vector<bool>
    {
    public:
        growing_vector() : data() {}

        std::vector<bool> data;

        std::vector<bool>::reference operator[](std::size_t idx)
        {
            if (idx >= data.size())
                data.resize((idx + 1) * 2, false);
            return data[idx];
        }
    };
}
