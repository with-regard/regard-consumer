The 'missing features' test framework
=====================================

This probably qualifies as experimental. This is an attempt to deal with the problem of deliberately 
missing features in a more TDD way. Every test in this framework should fail its assertions, so they
all have [ExpectedException(typeof(AssertionException)]. (Obviously unless the missing feature is
actually there to throw an exception)

This is intended to deal with situations where a piece of functionality is intended but we want to ship
without it. There should exist a test to ensure that we are shipping the intended behaviour, and it
hasn't changed without our knowledge.

However, it feels somehow wrong to write an assertion that something is broken. We should write the test
to show the intended behaviour, and verify that it's not happening. Grouping these tests into a separate
library gives them more meaning: these tests all assert that deliberately missing functionality is still
missing. They fail if the behaviour of the system changes.

Move the tests to the main test framework and remove the expected exception when implementing features. 
Ensure that unimplemented behaviour remains the same otherwise.

One consideration was to say that every test here should fail, but that allows the nature of the failure
to change (ie, if something is not supposed to return a particular value, it shouldn't crash instead).
Therefore, all these tests should be set up to pass, and they should be set up so that they require
trivial modification in order to check that one of these features is now working instead of broken.

I think traditional TDD would have us put these tests in the main framework and have them assert that
the feature doesn't work. The two modifications made here are that the test should be mainly written 
as if testing for the 'correct' functionality, so it can be re-used later, and it's possible to browse
this library to see things that are remaining to do.

This should keep 'unimplemented' behaviour from turning into 'undefined' behaviour, and make it easy
to differentiate between tests for the desired library behaviour and tests for behaviours that are
desired but not yet present.

This is possibly also a good place to put tests for known bugs. (If they're known, we should verify
that they still exist and exhibit the same behaviour, I think)
