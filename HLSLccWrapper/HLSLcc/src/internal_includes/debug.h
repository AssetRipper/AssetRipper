#ifndef DEBUG_H_
#define DEBUG_H_

#ifdef _DEBUG
#include "assert.h"
#define ASSERT(expr) CustomAssert(expr)
static void CustomAssert(int expression)
{
    if (!expression)
    {
        assert(0);
    }
}

#else
#define UNUSED(EXPR_) \
    do { if (false) (void)(EXPR_); } while(0)
#define ASSERT(expr) UNUSED(expr)
#endif

#endif
